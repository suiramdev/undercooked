#nullable enable

using Undercooked.Server;

namespace Undercooked.Editor;

public static class ConvexTestMenu
{
	[Menu( "Editor", "convex/Get My Profile" )]
	public static void GetMyProfile()
	{
		ConvexService.GetMyProfile();
	}

	[Menu( "Editor", "convex/Add Money" )]
	public static void AddMoney()
	{
		ConvexService.AddMoney( 1 );
	}
}

