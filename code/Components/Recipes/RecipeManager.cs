#nullable enable

using Undercooked.Components.Enums;
using Undercooked.Resources;

namespace Undercooked.Components;

public class RecipeManager : Component
{
	[Property]
	[Description( "The recipes that the player can make" )]
	public List<Recipe> AvailableRecipes { get; set; } = [];

	public Recipe? CheckRecipe( List<IngredientType> ingredients )
	{
		foreach ( var recipe in AvailableRecipes )
		{
			if ( IsRecipeMatch( ingredients, recipe ) )
			{
				return recipe;
			}
		}

		return null;
	}

	public static bool IsRecipeMatch( List<IngredientType> ingredients, Recipe recipe )
	{
		// First check if the number of ingredients is the same
		if ( recipe.RequiredIngredients.Count != ingredients.Count )
		{
			return false;
		}

		// Then check if all the ingredients are present
		return recipe.RequiredIngredients.All( ingredients.Contains );
	}
}

