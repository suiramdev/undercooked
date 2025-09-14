#nullable enable

using Sandbox.Citizen;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public abstract class ItemBase : Component, IPickable, IInteractable
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

	[Property]
	[Description( "The depositable that the item is on" )]
	[ReadOnly]
	public IDepositable? Depositable { get; set; }

	public virtual bool TryInteract( Player player )
	{
		// If the player's slot is not empty, we can't interact with the item
		if ( !player.PlayerSlot.Empty ) return false;

		// If the item can be picked up, we can deposit it into the player's slot
		if ( CanBePickedUp( player ) )
		{
			return player.PlayerSlot.TryDeposit( this, player );
		}

		return false;
	}

	public virtual bool TryAlternateInteract( Player player )
	{
		return false;
	}

	public virtual bool CanBePickedUp( Player player )
	{
		return Depositable is null;
	}

	public abstract bool CanBeDroppedOn( IDepositable depositable, Player player );

	public virtual void OnPickedUp( Player player )
	{
		Depositable = player.PlayerInteraction.PlayerSlot;
		Log.Info( $"Picking up pickable {this} on {Depositable}" );
	}

	public virtual void OnDroppedOn( IDepositable depositable, Player player )
	{
		Depositable = depositable;
		Log.Info( $"Dropping pickable {this} on {depositable}" );
	}

	public virtual void OnWorldDropped( Vector3 position, Rotation rotation )
	{
		Depositable = null;
		Log.Info( $"Dropping pickable {this}" );
	}
}
