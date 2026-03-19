#nullable enable

namespace Undercooked;

public static class BackendUserIdentity
{
	public static string ResolveUserId( Player player )
	{
		return ResolveUserId( player.Network.Owner );
	}

	public static string ResolveUserId( Connection? connection )
	{
		return connection?.Id.ToString( "N" ) ?? "local";
	}

	public static string ResolveDisplayName( Player player )
	{
		return ResolveDisplayName( player.Network.Owner );
	}

	public static string ResolveDisplayName( Connection? connection )
	{
		if ( connection is null )
			return "Local Player";

		if ( !string.IsNullOrWhiteSpace( connection.DisplayName ) )
			return connection.DisplayName;

		if ( !string.IsNullOrWhiteSpace( connection.Name ) )
			return connection.Name;

		return connection.Id.ToString( "N" );
	}
}
