#nullable enable

using Undercooked.Components.Interfaces;
using Undercooked.Components.Enums;

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
	protected IPickable? StoredPickable { get; set; }

	public bool Empty => StoredPickable is null;

	[Rpc.Host]
	public virtual void Interact( Player player )
	{
		var held = player.PlayerSlot.GetPickable();

		// Attempt to retrieve the stored item from the station
		if ( held is null )
		{
			var pickable = TakePickable();
			if ( pickable is null ) return;

			player.PlayerSlot.Deposit( pickable, player );
			return;
		}

		// Attempt to deposit the held item into the pickable currently stored on the station (if any)
		if ( StoredPickable is not null && StoredPickable is IDepositable depositable )
		{
			// Special case: If the held item is a transferable, allow transferring its contents to the stored pickable
			// e.g. If the player is holding a pan, they can transfer the contents to the pickable currently on the station
			if ( held is ITransferable transferable && !transferable.Empty )
			{
				transferable.TransferPickable( depositable, player );
				return;
			}

			// Attempt to deposit the held item into the pickable currently stored on the station
			depositable.Deposit( held, player );
		}

		// Attempt to deposit the held item into the station
		Deposit( held, player );
	}

	[Rpc.Host]
	public virtual void AlternateInteract( Player player )
	{
		return;
	}

	[Rpc.Host]
	public virtual void Deposit( IPickable pickable, Player by )
	{
		if ( StoredPickable is not null || pickable is not IPickable item ) return;

		StoredPickable = item;
		StoredPickable.GameObject.SetParent( Socket );
		StoredPickable.GameObject.LocalPosition = Vector3.Zero;
		StoredPickable.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = StoredPickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody is not null ) rigidbody.Enabled = false;

		Collider? collider = StoredPickable.GameObject.GetComponent<Collider>( true );
		if ( collider is not null ) collider.Enabled = false;

		// Notify the pickable that it was dropped on the counter
		pickable.OnDeposited( this, by );
	}

	public virtual IPickable? GetPickable() => StoredPickable;

	public virtual IPickable? TakePickable()
	{
		var temp = StoredPickable;
		StoredPickable = default;

		return temp;
	}
}
