#nullable enable

using System;
using Sandbox;
using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

[Icon( "inventory" )]
public class ChopCounter : Component, IDepositable, IUsable
{
	[Property]
	[Description( "The type of use for the chop counter" )]
	public UseType UseType { get; set; } = UseType.Hold;

	[Property]
	[Description( "This is a reference point for positioning the displayed item" )]
	public GameObject? ItemAnchorPoint { get; set; }

	[Property]
	[Description( "How fast the chopping progress increases per second" )]
	public float ChopSpeed { get; set; } = 0.2f;

	[Property]
	[ReadOnly]
	public IPickable? StoredPickable { get; set; }

	private TimeSince _lastChopTime = 0f;
	private const float CHOP_COOLDOWN = 0.1f;

	public bool CanAccept( IPickable pickable, Player player )
	{
		return StoredPickable == null;
	}

	public bool CanWithdraw( Player player )
	{
		return StoredPickable != null;
	}

	public void OnDeposit( IPickable pickable, Player player )
	{
		StoredPickable = pickable;

		StoredPickable.GameObject.SetParent( ItemAnchorPoint ?? GameObject );
		StoredPickable.GameObject.LocalPosition = Vector3.Zero;
		StoredPickable.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = StoredPickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody != null )
		{
			rigidbody.Enabled = false;
		}

		Collider? collider = StoredPickable.GameObject.GetComponent<Collider>( true );
		if ( collider != null )
		{
			collider.Enabled = false;
		}
	}

	public void OnWithdraw( IPickable pickable, Player player )
	{
		StoredPickable = null;
	}

	public IPickable? GetStoredPickable()
	{
		return StoredPickable;
	}

	public bool CanUse( Player player )
	{
		// Can use if there is an ingredient, it is choppable, it is raw, and the cooldown has passed
		return StoredPickable != null &&
			StoredPickable is Ingredient ingredient &&
			ingredient.Choppable &&
			ingredient.State == IngredientState.Raw &&
			_lastChopTime >= CHOP_COOLDOWN;
	}

	public void OnUse( Player player )
	{
		if ( StoredPickable == null || StoredPickable is not Ingredient ingredient )
		{
			return;
		}

		ingredient.ChopProgress = MathF.Min( ingredient.ChopProgress + ChopSpeed * CHOP_COOLDOWN, 1f );
		_lastChopTime = 0f;

		if ( ingredient.ChopProgress >= 1f )
		{
			ingredient.SetState( IngredientState.Chopped );
		}
	}
}
