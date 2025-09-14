#nullable enable

namespace Undercooked.Components;

[Icon( "person" )]
public class Player : Component
{
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
}
