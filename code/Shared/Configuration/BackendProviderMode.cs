#nullable enable

using System;

namespace Undercooked;

public enum BackendProviderMode
{
	LocalServer,
	RestApi
}

public static class BackendProviderModeParser
{
	public static BackendProviderMode Parse( string? value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return BackendProviderMode.LocalServer;

		if ( value.Equals( "rest", StringComparison.OrdinalIgnoreCase ) ||
			value.Equals( "restapi", StringComparison.OrdinalIgnoreCase ) ||
			value.Equals( "api", StringComparison.OrdinalIgnoreCase ) )
		{
			return BackendProviderMode.RestApi;
		}

		return BackendProviderMode.LocalServer;
	}
}
