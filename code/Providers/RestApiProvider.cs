#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Undercooked;

public sealed class RestApiProvider : IUserDataService
{
	public Task<UserDataSnapshot> GetUserDataAsync( GetUserDataRequest request, CancellationToken cancellationToken = default )
	{
		return SendAsync( "get", request, cancellationToken );
	}

	public Task<UserDataSnapshot> SetMoneyAsync( SetMoneyRequest request, CancellationToken cancellationToken = default )
	{
		return SendAsync( "set-money", request, cancellationToken );
	}

	public Task<UserDataSnapshot> AddMoneyAsync( AddMoneyRequest request, CancellationToken cancellationToken = default )
	{
		return SendAsync( "add-money", request, cancellationToken );
	}

	private static Task<UserDataSnapshot> SendAsync<TRequest>(
		string route,
		TRequest request,
		CancellationToken cancellationToken )
	{
		return Http.RequestJsonAsync<UserDataSnapshot>(
			BuildUrl( route ),
			"POST",
			Http.CreateJsonContent( request ),
			BuildHeaders(),
			cancellationToken );
	}

	private static string BuildUrl( string route )
	{
		return $"{BackendConVars.RestApiBaseUrl.TrimEnd( '/' )}/{route}";
	}

	private static Dictionary<string, string>? BuildHeaders()
	{
		if ( string.IsNullOrWhiteSpace( BackendConVars.RestApiKey ) )
			return null;

		return new Dictionary<string, string>
		{
			["X-Api-Key"] = BackendConVars.RestApiKey
		};
	}
}
