#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Undercooked;

[Icon( "cloud_sync" )]
[Title( "Backend Gateway" )]
public sealed class BackendGateway : Component, IUserDataService
{
	private readonly Dictionary<Guid, TaskCompletionSource<UserDataSnapshot>> _pendingRequests = new();

	public static BackendGateway? Instance { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Instance == this )
		{
			Instance = null;
		}

		foreach ( var pending in _pendingRequests.Values )
		{
			pending.TrySetCanceled();
		}

		_pendingRequests.Clear();
	}

	public async Task<UserDataSnapshot> GetUserDataAsync( GetUserDataRequest request, CancellationToken cancellationToken = default )
	{
		if ( Networking.IsHost )
		{
			var snapshot = await ResolveRuntime().UserDataService.GetUserDataAsync( request, cancellationToken );
			ApplySnapshotToConnection( Connection.Local, snapshot );
			return snapshot;
		}

		return await QueueRequestAsync(
			requestId => RequestGetUserData( requestId, request.UserId, request.DisplayName ),
			cancellationToken );
	}

	public async Task<UserDataSnapshot> SetMoneyAsync( SetMoneyRequest request, CancellationToken cancellationToken = default )
	{
		if ( Networking.IsHost )
		{
			var snapshot = await ResolveRuntime().UserDataService.SetMoneyAsync( request, cancellationToken );
			ApplySnapshotToConnection( Connection.Local, snapshot );
			return snapshot;
		}

		return await QueueRequestAsync(
			requestId => RequestSetMoney( requestId, request.UserId, request.Amount, request.Reason, request.DisplayName ),
			cancellationToken );
	}

	public async Task<UserDataSnapshot> AddMoneyAsync( AddMoneyRequest request, CancellationToken cancellationToken = default )
	{
		if ( Networking.IsHost )
		{
			var snapshot = await ResolveRuntime().UserDataService.AddMoneyAsync( request, cancellationToken );
			ApplySnapshotToConnection( Connection.Local, snapshot );
			return snapshot;
		}

		return await QueueRequestAsync(
			requestId => RequestAddMoney( requestId, request.UserId, request.Amount, request.Reason, request.DisplayName ),
			cancellationToken );
	}

	[Rpc.Host]
	private void RequestGetUserData( Guid requestId, string userId, string displayName )
	{
		var caller = Rpc.Caller ?? Connection.Local;
		_ = ExecuteRequestAsync(
			caller,
			requestId,
			() => ResolveRuntime().UserDataService.GetUserDataAsync( new GetUserDataRequest
			{
				UserId = userId,
				DisplayName = displayName
			} ) );
	}

	[Rpc.Host]
	private void RequestSetMoney( Guid requestId, string userId, int amount, string reason, string displayName )
	{
		var caller = Rpc.Caller ?? Connection.Local;
		_ = ExecuteRequestAsync(
			caller,
			requestId,
			() => ResolveRuntime().UserDataService.SetMoneyAsync( new SetMoneyRequest
			{
				UserId = userId,
				Amount = amount,
				Reason = reason,
				DisplayName = displayName
			} ) );
	}

	[Rpc.Host]
	private void RequestAddMoney( Guid requestId, string userId, int amount, string reason, string displayName )
	{
		var caller = Rpc.Caller ?? Connection.Local;
		_ = ExecuteRequestAsync(
			caller,
			requestId,
			() => ResolveRuntime().UserDataService.AddMoneyAsync( new AddMoneyRequest
			{
				UserId = userId,
				Amount = amount,
				Reason = reason,
				DisplayName = displayName
			} ) );
	}

	[Rpc.Broadcast]
	private void ReceiveResponse(
		Guid requestId,
		bool succeeded,
		string userId,
		string displayName,
		int money,
		long revision,
		float updatedAt,
		string errorMessage )
	{
		if ( !_pendingRequests.TryGetValue( requestId, out var pending ) )
			return;

		_pendingRequests.Remove( requestId );

		if ( !succeeded )
		{
			pending.TrySetException( new InvalidOperationException( errorMessage ) );
			return;
		}

		pending.TrySetResult( new UserDataSnapshot
		{
			UserId = userId,
			DisplayName = displayName,
			Money = money,
			Revision = revision,
			UpdatedAt = updatedAt
		} );
	}

	private async Task ExecuteRequestAsync(
		Connection caller,
		Guid requestId,
		Func<Task<UserDataSnapshot>> operation )
	{
		try
		{
			var snapshot = await operation();
			ApplySnapshotToConnection( caller, snapshot );

			using ( Rpc.FilterInclude( caller ) )
			{
				ReceiveResponse(
					requestId,
					true,
					snapshot.UserId,
					snapshot.DisplayName,
					snapshot.Money,
					snapshot.Revision,
					snapshot.UpdatedAt,
					string.Empty );
			}
		}
		catch ( Exception exception )
		{
			using ( Rpc.FilterInclude( caller ) )
			{
				ReceiveResponse( requestId, false, string.Empty, string.Empty, 0, 0L, 0f, exception.Message );
			}
		}
	}

	private Task<UserDataSnapshot> QueueRequestAsync( Action<Guid> send, CancellationToken _ )
	{
		var requestId = Guid.NewGuid();
		var pending = new TaskCompletionSource<UserDataSnapshot>();
		_pendingRequests[requestId] = pending;

		send( requestId );

		return pending.Task;
	}

	private static BackendRuntime ResolveRuntime()
	{
		var scene = Game.ActiveScene ?? throw new InvalidOperationException( "No active scene is available for backend operations." );
		return BackendRuntime.Instance ?? BackendRuntime.EnsureAvailable( scene );
	}

	private static void ApplySnapshotToConnection( Connection? connection, UserDataSnapshot snapshot )
	{
		if ( connection is null )
			return;

		var scene = Game.ActiveScene;
		if ( scene is null )
			return;

		var player = scene
			.GetAllComponents<Player>()
			.FirstOrDefault( candidate => candidate.Network.OwnerId == connection.Id );

		player?.ApplyBackendUserData( snapshot );
	}
}
