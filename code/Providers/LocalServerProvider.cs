#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Undercooked;

public sealed class LocalServerProvider : IUserDataService
{
	private readonly IUserDataStore _userDataStore;

	public LocalServerProvider( IUserDataStore userDataStore )
	{
		_userDataStore = userDataStore;
	}

	public async Task<UserDataSnapshot> GetUserDataAsync( GetUserDataRequest request, CancellationToken cancellationToken = default )
	{
		ValidateUserId( request.UserId );
		var record = await _userDataStore.GetOrCreateAsync( request.UserId, request.DisplayName, cancellationToken );
		return record.ToSnapshot();
	}

	public async Task<UserDataSnapshot> SetMoneyAsync( SetMoneyRequest request, CancellationToken cancellationToken = default )
	{
		ValidateUserId( request.UserId );
		var record = await _userDataStore.GetOrCreateAsync( request.UserId, request.DisplayName, cancellationToken );
		record.Money = request.Amount;
		if ( !string.IsNullOrWhiteSpace( request.DisplayName ) )
			record.DisplayName = request.DisplayName;

		record = await _userDataStore.SaveAsync( record, cancellationToken );
		return record.ToSnapshot();
	}

	public async Task<UserDataSnapshot> AddMoneyAsync( AddMoneyRequest request, CancellationToken cancellationToken = default )
	{
		ValidateUserId( request.UserId );
		var record = await _userDataStore.GetOrCreateAsync( request.UserId, request.DisplayName, cancellationToken );
		record.Money += request.Amount;
		if ( !string.IsNullOrWhiteSpace( request.DisplayName ) )
			record.DisplayName = request.DisplayName;

		record = await _userDataStore.SaveAsync( record, cancellationToken );
		return record.ToSnapshot();
	}

	private static void ValidateUserId( string userId )
	{
		if ( string.IsNullOrWhiteSpace( userId ) )
			throw new ArgumentException( "User id is required.", nameof( userId ) );
	}
}
