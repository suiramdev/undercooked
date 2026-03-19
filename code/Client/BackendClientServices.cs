#nullable enable

using System;
using System.Linq;

namespace Undercooked;

public static class BackendClientServices
{
	private static IUserDataService? _overrideUserDataService;

	public static IUserDataService UserData =>
		_overrideUserDataService ??
		TryResolveGateway() ??
		throw new InvalidOperationException( "Backend gateway is not available in the active scene." );

	public static void OverrideUserDataService( IUserDataService userDataService )
	{
		_overrideUserDataService = userDataService;
	}

	public static void ResetOverrides()
	{
		_overrideUserDataService = null;
	}

	public static bool TryGetUserDataService( out IUserDataService? userDataService )
	{
		userDataService = _overrideUserDataService ?? TryResolveGateway();
		return userDataService is not null;
	}

	private static BackendGateway? TryResolveGateway()
	{
		var scene = Game.ActiveScene;
		if ( scene is null )
			return null;

		return scene.GetAllComponents<BackendGateway>().FirstOrDefault();
	}
}
