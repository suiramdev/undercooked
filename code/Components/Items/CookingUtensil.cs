#nullable enable

using System;
using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public class CookingUtensil : Utensil
{
	[Property]
	[Description( "The attachment object of the utensil" )]
	public GameObject? ItemAnchorPoint { get; set; }

	[Property]
	[Description( "The ingredients that the utensil contains" )]
	[ReadOnly]
	public IngredientItem? Ingredient { get; set; }

	[Property]
	[Description( "The progress of the utensil's cooking" )]
	[ReadOnly]
	public float CookProgress { get => Ingredient?.CookProgress ?? 0f; }

	public override bool CanAccept( IPickable pickable, Player player )
	{
		return pickable is IngredientItem ingredient && ingredient.Cookable;
	}

	public override bool CanWithdraw( Player player )
	{
		return false;
	}

	public override void OnDeposit( IPickable pickable, Player player )
	{
		if ( pickable is not IngredientItem ingredient )
		{
			return;
		}

		Ingredient = ingredient;
		ingredient.GameObject.SetParent( ItemAnchorPoint ?? GameObject );
		ingredient.GameObject.LocalPosition = Vector3.Zero;
		ingredient.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = ingredient.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody != null )
		{
			rigidbody.Enabled = false;
		}

		Collider? collider = ingredient.GameObject.GetComponent<Collider>( true );
		if ( collider != null )
		{
			collider.Enabled = false;
		}
	}

	public override void OnWithdraw( IPickable pickable, Player player )
	{
	}

	public override IPickable? GetStoredPickable()
	{
		return Ingredient;
	}

	public override bool CanPickup( Player player )
	{
		return true;
	}

	public override bool CanDrop( Player player )
	{
		return true;
	}

}
