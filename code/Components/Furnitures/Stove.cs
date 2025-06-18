#nullable enable

using System;
using Sandbox;
using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

[Icon( "inventory" )]
public class Stove : Component, IDepositable
{
	[Property]
	[Description( "This is a reference point for positioning the displayed item" )]
	public GameObject? ItemAnchorPoint { get; set; }

	[Property]
	[Description( "How many seconds it takes to fully cook an ingredient" )]
	public float CookingTime { get; set; } = 5f;

	[Property]
	[Description( "How many seconds after being cooked before the ingredient burns" )]
	public float OvercookTime { get; set; } = 2.5f;

	[Property]
	[ReadOnly]
	public IPickable? StoredPickable { get; set; }

	protected override void OnFixedUpdate()
	{
		TryCook();
	}

	private void TryCook()
	{
		if ( StoredPickable == null || StoredPickable is not CookingUtensil utensil )
		{
			return;
		}

		Ingredient? ingredient = utensil.Ingredient;

		// Check if the utensil has an ingredient and if it is in the correct state
		// Also check if the utensil is not already burnt
		if ( ingredient == null || !ingredient.Cookable )
		{
			return;
		}

		// Calculate cook speed based on cooking time
		float cookSpeed = 1f / CookingTime;

		// Calculate the burn threshold based on overcook time
		float burnThreshold = 1f + (OvercookTime * cookSpeed);

		// Cook the ingredient smoothly over time
		ingredient.CookProgress = MathF.Min( ingredient.CookProgress + cookSpeed * Time.Delta, burnThreshold );

		// Set ingredient state based on cook progress
		if ( ingredient.CookProgress >= burnThreshold )
		{
			ingredient.SetState( IngredientState.Burnt );
		}
		else if ( ingredient.CookProgress >= 1f )
		{
			ingredient.SetState( IngredientState.Cooked );
		}
	}

	public bool CanAccept( IPickable pickable, Player player )
	{
		return StoredPickable == null || (StoredPickable is CookingUtensil utensil && utensil.CanAccept( pickable, player ));
	}

	public bool CanWithdraw( Player player )
	{
		return StoredPickable != null;
	}

	public void OnDeposit( IPickable pickable, Player player )
	{
		if ( StoredPickable != null && StoredPickable is CookingUtensil utensil )
		{
			utensil.OnDeposit( pickable, player );
			return;
		}

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
}
