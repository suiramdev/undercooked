#nullable enable

using Undercooked.Server;

namespace Undercooked.Editor;

public static class ConvexTestMenu
{
	[Menu( "Editor", "convex/Get My Profile" )]
	public static void GetMyProfile()
	{
		Convex.GetMyProfile();
	}

	[Menu( "Editor", "convex/Add Money" )]
	public static void AddMoney()
	{
		Convex.AddMoney( 1 );
	}
}

