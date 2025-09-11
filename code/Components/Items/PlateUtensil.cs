#nullable enable

using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;
using Undercooked.Resources;

namespace Undercooked.Components;

public sealed class PlateUtensil : Utensil
{
	[Property]
	[Description( "The anchor point of the plate" )]
	public GameObject? ItemAnchorPoint { get; set; }

	[Property]
	[Description( "The ingredients that the plate contains" )]
	[ReadOnly]
	public List<IngredientResource> Ingredients { get; set; } = [];

	[Property]
	[Description( "The recipe that the plate contains" )]
	[ReadOnly]
	public RecipeResource? Recipe { get; set; }

	private GameObject? _recipeResultObject;

	public override bool CanAccept( IPickable pickable, Player player )
	{
		return pickable is IngredientItem ingredient && RecipeManager.Instance.CanAddIngredient( ingredient.Resource, Ingredients );
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

		Ingredients.Add( ingredient.Resource );
		ingredient.GameObject.Destroy();

		// Get the recipe that the plate can make
		Recipe = RecipeManager.Instance.GetRecipeFromIngredients( Ingredients );

		if ( Recipe != null )
		{
			_recipeResultObject?.Destroy();

			_recipeResultObject = GameObject.Clone( Recipe.Result );
			_recipeResultObject.SetParent( ItemAnchorPoint ?? GameObject );
			_recipeResultObject.LocalPosition = Vector3.Zero;
			_recipeResultObject.LocalRotation = Rotation.Identity;
			_recipeResultObject.NetworkSpawn();
		}
	}

	public override void OnWithdraw( IPickable pickable, Player player )
	{
		// Can't withdraw from a plate
	}

	public override IPickable? GetStoredPickable()
	{
		return null; // Can't withdraw from a plate
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
