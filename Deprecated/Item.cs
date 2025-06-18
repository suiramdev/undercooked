using Sandbox;
using Sandbox.Citizen;
using Undercooked.Resources.Enums;

namespace Undercooked.Resources;

[GameResource( "Item", "item", "An item in the game", Icon = "category" )]
public class Item : GameResource
{
	public string Name { get; set; }

	public ItemType ItemType { get; set; }

	[Group( "Display" )]
	public Model Model { get; set; }

	[Group( "Display" )]
	public string AttachmentBone { get; set; } = "hand_r";

	[Group( "Display" )]
	public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.HoldItem;

	[Group( "Display" )]
	public Vector3 AttachmentOffset { get; set; }

	[Group( "Display" )]
	public Rotation AttachmentRotation { get; set; }
}
