#nullable enable

using Undercooked.Components.Interfaces;
using Undercooked.Components.Enums;

namespace Undercooked.Components;

[Icon( "inventory" )]
public abstract class StationBase<T> : Component, IDepositable, IInteractable where T : IPickable
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
	protected T? StoredPickable { get; set; }

	public bool Empty => StoredPickable is null;

	public virtual bool TryInteract( Player player )
	{
		var held = player.PlayerSlot.GetPickable();
		if ( held is null )
		{
			var pickable = TakePickable();
			if ( pickable is null ) return false;

			return player.PlayerSlot.TryDeposit( pickable, player );
		}

		if ( StoredPickable is not null && StoredPickable is IDepositable depositable )
		{
			if ( depositable.TryDeposit( held, player ) )
			{
				player.PlayerSlot.TakePickable();
				return true;
			}
		}

		if ( TryDeposit( held, player ) )
		{
			player.PlayerSlot.TakePickable();
			return true;
		}

		return false;
	}

	public virtual bool TryAlternateInteract( Player player )
	{
		return false;
	}

	public virtual bool TryDeposit( IPickable pickable, Player by )
	{
		if ( StoredPickable is not null || pickable is not T item ) return false;

		StoredPickable = item;
		StoredPickable.GameObject.SetParent( Socket );
		StoredPickable.GameObject.LocalPosition = Vector3.Zero;
		StoredPickable.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = StoredPickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody is not null ) rigidbody.Enabled = false;

		Collider? collider = StoredPickable.GameObject.GetComponent<Collider>( true );
		if ( collider is not null ) collider.Enabled = false;

		// Notify the pickable that it was dropped on the counter
		pickable.OnDroppedOn( this, by );

		return true;
	}

	public virtual IPickable? GetPickable() => StoredPickable;

	public virtual IPickable? TakePickable()
	{
		var temp = StoredPickable;
		StoredPickable = default;

		return temp;
	}
}
