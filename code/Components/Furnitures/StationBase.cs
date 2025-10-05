#nullable enable

using Undercooked.Components.Interfaces;
using Undercooked.Components.Enums;
using System.Runtime.InteropServices;

namespace Undercooked.Components;

[Icon( "inventory" )]
public abstract class StationBase : Component, IDepositable, IInteractable
{
	[Property]
	[Description( "The type of interaction for the station" )]
	public virtual InteractionType InteractionType { get; set; } = InteractionType.Press;

	[Property]
	[Description( "The type of interaction for the station" )]
	public virtual InteractionType AlternateInteractionType { get; set; } = InteractionType.Press;

	[Property]
	[Description( "The socket to deposit the pickable into" )]
	public required GameObject Socket { get; set; }

	[Property]
	[Description( "The pickable that is stored on the station" )]
	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	public IPickable? StoredPickable { get; protected set; }

	[Rpc.Host]
	public virtual void TryInteract( Player by )
	{
		var held = by.PlayerSlot.StoredPickable;

		// Attempt to retrieve the stored item from the station
		if ( held is null && StoredPickable is not null )
		{
			if ( by.PlayerSlot.CanAccept( StoredPickable ) )
			{
				var pickable = StoredPickable;
				StoredPickable = null;
				by.PlayerSlot.TryDeposit( pickable );
			}
			return;
		}

		// Attempt to deposit the held item into the pickable currently stored on the station (if any)
		if ( StoredPickable is not null && StoredPickable is IDepositable storedDepositable )
		{
			// Special case: If the held item is a transferable, allow transferring its contents to the stored pickable
			// e.g. If the player is holding a pan, they can transfer the contents to the pickable currently on the station
			if ( held is ITransferable transferable )
			{
				transferable.TryTransfer( storedDepositable );
				return;
			}
		}

		// Attempt to deposit the held item onto the station
		if ( held is not null )
		{
			by.PlayerSlot.TryTransferTo( this );
		}
	}

	[Rpc.Host]
	public virtual void TryAlternateInteract( Player by )
	{
		return;
	}

	public virtual bool CanAccept( IPickable pickable )
	{
		return StoredPickable is null;
	}

	[Rpc.Host]
	public virtual void TryDeposit( IPickable pickable )
	{
		if ( !CanAccept( pickable ) ) return;

		StoredPickable = pickable;
		StoredPickable.GameObject.SetParent( Socket );
		StoredPickable.GameObject.LocalPosition = Vector3.Zero;
		StoredPickable.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = StoredPickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody is not null ) rigidbody.Enabled = false;

		Collider? collider = StoredPickable.GameObject.GetComponent<Collider>( true );
		if ( collider is not null ) collider.Enabled = false;

		pickable.OnDeposit( this );
	}
}
