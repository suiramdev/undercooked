#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Undercooked;

public partial class Player
{
	[Sync( SyncFlags.FromHost )]
	public string BackendUserId { get; private set; } = string.Empty;

	[Sync( SyncFlags.FromHost )]
	public string BackendDisplayName { get; private set; } = string.Empty;

	[Sync( SyncFlags.FromHost )]
	public int BackendMoney { get; private set; }

	[Sync( SyncFlags.FromHost )]
	public long BackendDataRevision { get; private set; }

	public bool HasBackendProfile => !string.IsNullOrWhiteSpace( BackendUserId );

	internal void ApplyBackendUserData( UserDataSnapshot snapshot )
	{
		BackendUserId = snapshot.UserId;
		BackendDisplayName = snapshot.DisplayName;
		BackendMoney = snapshot.Money;
		BackendDataRevision = snapshot.Revision;
	}

	public string ResolveBackendUserId()
	{
		return BackendUserIdentity.ResolveUserId( this );
	}

	public string ResolveBackendDisplayName()
	{
		return BackendUserIdentity.ResolveDisplayName( this );
	}

	private void InitializeBackendRuntime()
	{
		if ( !Networking.IsHost || Scene is null )
			return;

		BackendRuntime.EnsureAvailable( Scene );
	}

	private void InitializeBackendClient()
	{
		if ( IsProxy )
			return;

		_ = RefreshBackendProfileAsync();
	}

	public async Task<UserDataSnapshot?> RefreshBackendProfileAsync( CancellationToken cancellationToken = default )
	{
		var service = await WaitForUserDataServiceAsync( cancellationToken );
		if ( service is null )
			return null;

		return await service.GetUserDataAsync(
			ResolveBackendUserId(),
			ResolveBackendDisplayName(),
			cancellationToken );
	}

	public async Task<UserDataSnapshot?> SetMoneyAsync(
		int amount,
		string reason = "",
		CancellationToken cancellationToken = default )
	{
		var service = await WaitForUserDataServiceAsync( cancellationToken );
		if ( service is null )
			return null;

		return await service.SetMoneyAsync(
			ResolveBackendUserId(),
			amount,
			reason,
			ResolveBackendDisplayName(),
			cancellationToken );
	}

	private static async Task<IUserDataService?> WaitForUserDataServiceAsync( CancellationToken cancellationToken )
	{
		for ( var attempt = 0; attempt < 50; attempt++ )
		{
			if ( BackendClientServices.TryGetUserDataService( out var service ) && service is not null )
				return service;

			await GameTask.DelayRealtime( 100, cancellationToken );
		}

		Log.Warning( "Backend user data service was not available in time." );
		return null;
	}
}
