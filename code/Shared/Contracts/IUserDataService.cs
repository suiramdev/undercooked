#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Undercooked;

public interface IUserDataService
{
	Task<UserDataSnapshot> GetUserDataAsync( GetUserDataRequest request, CancellationToken cancellationToken = default );

	Task<UserDataSnapshot> SetMoneyAsync( SetMoneyRequest request, CancellationToken cancellationToken = default );

	Task<UserDataSnapshot> AddMoneyAsync( AddMoneyRequest request, CancellationToken cancellationToken = default );
}

public static class UserDataServiceExtensions
{
	public static Task<UserDataSnapshot> GetUserDataAsync(
		this IUserDataService service,
		string userId,
		string? displayName = null,
		CancellationToken cancellationToken = default )
	{
		return service.GetUserDataAsync( new GetUserDataRequest
		{
			UserId = userId,
			DisplayName = displayName ?? string.Empty
		}, cancellationToken );
	}

	public static Task<UserDataSnapshot> SetMoneyAsync(
		this IUserDataService service,
		string userId,
		int amount,
		string? reason = null,
		string? displayName = null,
		CancellationToken cancellationToken = default )
	{
		return service.SetMoneyAsync( new SetMoneyRequest
		{
			UserId = userId,
			Amount = amount,
			Reason = reason ?? string.Empty,
			DisplayName = displayName ?? string.Empty
		}, cancellationToken );
	}

	public static Task<UserDataSnapshot> AddMoneyAsync(
		this IUserDataService service,
		string userId,
		int amount,
		string? reason = null,
		string? displayName = null,
		CancellationToken cancellationToken = default )
	{
		return service.AddMoneyAsync( new AddMoneyRequest
		{
			UserId = userId,
			Amount = amount,
			Reason = reason ?? string.Empty,
			DisplayName = displayName ?? string.Empty
		}, cancellationToken );
	}
}
