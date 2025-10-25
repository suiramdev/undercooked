#nullable enable

namespace Undercooked;

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

	public virtual string? GetInteractionText( Player by )
	{
		var held = by.StoredPickable;

		if ( held is null && StoredPickable is not null )
		{
			return "Pickup";
		}

		if ( held is not null )
		{
			return "Deposit";
		}

		return null;
	}

	[Rpc.Host]
	public virtual void TryInteract( Player by )
	{
		var held = by.StoredPickable;

		// Attempt to retrieve the stored item from the station
		if ( held is null && StoredPickable is not null )
		{
			if ( by.CanAccept( StoredPickable ) )
			{
				var pickable = StoredPickable;
				StoredPickable = null;
				by.TryDeposit( pickable );
			}
			return;
		}

		// If holding something and station has a depositable item, try interacting with nested depositable
		if ( held is not null && StoredPickable is IDepositable nestedDepositable )
		{
			// If holding a transferable (like frying pan), try transferring its contents to the nested item
			if ( held is ITransferable transferable )
			{
				transferable.TryTransfer( nestedDepositable );
				return;
			}

			// If holding a direct item (like an ingredient), try depositing into the nested item
			if ( nestedDepositable.CanAccept( held ) )
			{
				by.TryTransfer( nestedDepositable );
				return;
			}
		}

		// Default: try depositing the held item into the station itself
		by.TryTransfer( this );
	}

	public virtual string? GetAlternateInteractionText( Player by )
	{
		return null;
	}

	[Rpc.Host]
	public virtual void TryAlternateInteract( Player by )
	{
		return;
	}

	public virtual bool CanAccept( IPickable pickable )
	{
		return StoredPickable is null || (StoredPickable is IDepositable depositable && depositable.CanAccept( pickable ));
	}

	[Rpc.Host]
	public virtual void TryDeposit( IPickable pickable )
	{
		if ( !CanAccept( pickable ) ) return;
		if ( StoredPickable is IDepositable depositable )
		{
			depositable.TryDeposit( pickable );
			return;
		}

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
