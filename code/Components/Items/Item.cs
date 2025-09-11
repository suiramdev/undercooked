#nullable enable

using Sandbox;
using Sandbox.Citizen;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public abstract class Item : Component, IPickable
{
	[Property]
	[Description( "The bone to attach the item to" )]
	public string AttachmentBone { get; set; } = "hand_r";

	[Property]
	[Description( "The type of hold to use for the item" )]
	public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.HoldItem;

	[Property]
	[Description( "The offset of the item from the attachment bone" )]
	public Vector3 AttachmentOffset { get; set; }

	[Property]
	[Description( "The rotation of the item from the attachment bone" )]
	public Rotation AttachmentRotation { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required ModelRenderer ModelRenderer { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required ModelCollider ModelCollider { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required Rigidbody Rigidbody { get; set; }

	public abstract bool CanPickup( Player player );

	public abstract bool CanDrop( Player player );
}
