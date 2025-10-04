#nullable enable

using System;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public class StoveStation : StationBase
{
	[Property]
	[Description( "How many seconds it takes to fully cook an ingredient" )]
	public float CookingTime { get; set; } = 5f;

	[Property]
	[Description( "How many seconds after being cooked before the ingredient burns" )]
	public float OvercookTime { get; set; } = 2.5f;

	protected override void OnFixedUpdate()
	{
		TryCook();
	}

	public override bool TryDeposit( IPickable pickable, Player by )
	{
		if ( pickable is not FryingPanItem fryingPan ) return false;
		return base.TryDeposit( fryingPan, by );
	}

	private void TryCook()
	{
		if ( StoredPickable is not FryingPanItem fryingPan ) return;

		IngredientItem? ingredient = fryingPan.Ingredient;

		// Check if the utensil has an ingredient and if it is in the correct state
		// Also check if the utensil is not already burnt
		if ( ingredient is null || !ingredient.Cookable )
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
			ingredient.OnBurned();
		}
		else if ( ingredient.CookProgress >= 1f )
		{
			ingredient.OnCooked();
		}
	}

}
