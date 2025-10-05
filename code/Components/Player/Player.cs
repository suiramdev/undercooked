#nullable enable

namespace Undercooked.Components;

[Icon( "person" )]
public class Player : Component
{
	public static Player? Local { get; private set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required PlayerController PlayerController { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required PlayerInteraction PlayerInteraction { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required PlayerSlot PlayerSlot { get; set; }

	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			// Assign this instance as the local player
			Local = this;
		}
	}

	protected override void OnDestroy()
	{
		if ( Local == this )
		{
			Local = null;
		}
	}
}
