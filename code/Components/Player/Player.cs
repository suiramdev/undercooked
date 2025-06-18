using Sandbox;

namespace Undercooked.Components;

[Icon( "person" )]
public class Player : Component
{
	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public PlayerController PlayerController { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public PlayerUse PlayerUse { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public PlayerPickup PlayerPickup { get; set; }
}
