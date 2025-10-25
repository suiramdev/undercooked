#nullable enable

using Sandbox.Citizen;

namespace Undercooked;

public abstract class ItemBase : Component, IPickable, IInteractable
{
	[Property]
	[Description( "The type of interaction for the item" )]
	public virtual InteractionType InteractionType { get; set; } = InteractionType.Press;

	[Property]
	[Description( "The type of interaction for the item" )]
	public virtual InteractionType AlternateInteractionType { get; set; } = InteractionType.Press;

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

	[Property]
	[Description( "The depositable that the item is on" )]
	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	public IDepositable? Depositable { get; set; }

	public virtual string? GetInteractionText( Player player ) => "Pickup";

	[Rpc.Host]
	public virtual void TryInteract( Player player )
	{
		player.TryDeposit( this );
	}

	public virtual string? GetAlternateInteractionText( Player player ) => null;

	[Rpc.Host]
	public virtual void TryAlternateInteract( Player player )
	{
		return;
	}

	public virtual bool CanBeDepositedOn( IDepositable depositable, Player player )
	{
		return true;
	}

	[Rpc.Host]
	public virtual void OnDeposit( IDepositable depositable )
	{
		Depositable = depositable;
	}

	[Rpc.Host]
	public virtual void OnDrop()
	{
		Depositable = null;
	}
}
