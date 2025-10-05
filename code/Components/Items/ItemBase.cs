#nullable enable

using Sandbox.Citizen;
using Undercooked.Components.Interfaces;
using Undercooked.Components.Enums;

namespace Undercooked.Components;

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

	[Rpc.Host]
	public virtual void Interact( Player player )
	{
		// If the player's slot is not empty, we can't interact with the item
		if ( !player.PlayerSlot.Empty ) return;

		// If the item can be picked up, we can deposit it into the player's slot
		if ( CanBePickedUp( player ) )
		{
			player.PlayerSlot.Deposit( this, player );
			return;
		}
	}

	[Rpc.Host]
	public virtual void AlternateInteract( Player player )
	{
		return;
	}

	public virtual bool CanBePickedUp( Player player )
	{
		return Depositable is null;
	}

	public abstract bool CanBeDepositedOn( IDepositable depositable, Player player );

	public virtual void OnPickedUp( Player player )
	{
		Depositable = player.PlayerInteraction.PlayerSlot;
	}

	[Rpc.Host]
	public virtual void OnDeposited( IDepositable depositable, Player player )
	{
		if ( Depositable is PlayerSlot playerSlot )
		{
			playerSlot.TakePickable();
		}

		Depositable = depositable;
	}

	[Rpc.Host]
	public virtual void OnDropped()
	{
		if ( Depositable is PlayerSlot playerSlot )
		{
			playerSlot.TakePickable();
		}

		Depositable = null;
	}
}
