#nullable enable

using System;
using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public class CuttingBoardStation : StationBase<IngredientItem>
{
	[Property]
	[Description( "The type of use for the chop counter" )]
	public UseType UseType { get; set; } = UseType.Hold;

	[Property]
	[Description( "How fast the chopping progress increases per second" )]
	public float ChopSpeed { get; set; } = 0.2f;

	private TimeSince _lastChopTime = 0f;
	private const float CHOP_COOLDOWN = 0.1f;

	public override bool TryInteract( Player by )
	{
		return false;
	}

	public override bool TryAlternateInteract( Player by )
	{
		if ( StoredPickable is null ) return false;

		if ( !StoredPickable.Choppable || _lastChopTime < CHOP_COOLDOWN )
		{
			return false;
		}

		StoredPickable.ChopProgress = MathF.Min( StoredPickable.ChopProgress + ChopSpeed * CHOP_COOLDOWN, 1f );
		_lastChopTime = 0f;

		if ( StoredPickable.ChopProgress >= 1f )
		{
			StoredPickable.OnChopped();
		}

		return true;
	}

	public override bool TryDeposit( IPickable pickable, Player by )
	{
		if ( StoredPickable is null || pickable is not IngredientItem ingredient ) return false;

		StoredPickable = ingredient;
		StoredPickable.GameObject.SetParent( Socket );
		StoredPickable.GameObject.LocalPosition = Vector3.Zero;
		StoredPickable.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = StoredPickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody is not null ) rigidbody.Enabled = false;

		Collider? collider = StoredPickable.GameObject.GetComponent<Collider>( true );
		if ( collider is not null ) collider.Enabled = false;

		// Notify the pickable that it was dropped on the cutting board
		pickable.OnDroppedOn( this, by );

		return true;
	}

	public override IPickable? GetPickable() => StoredPickable;

	public override IPickable? TakePickable()
	{
		var temp = StoredPickable;
		StoredPickable = default;
		return temp;
	}
}
