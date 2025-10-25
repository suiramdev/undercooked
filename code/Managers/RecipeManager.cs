#nullable enable

namespace Undercooked;

public class RecipeManager : Component
{
	public static RecipeManager Instance { get; private set; } = null!;

	public RecipeManager() : base()
	{
		Instance = this;
	}

	/// <summary>
	/// Gets a list of recipes that include the given ingredients
	/// </summary>
	/// <param name="ingredients"></param>
	/// <returns>A list of recipes that include the given ingredients</returns>
	public List<RecipeResource> GetRecipesIncludingIngredients( List<IngredientResource> ingredients )
	{
		return LevelConfig.Instance.CookableRecipes.Where( recipe => ingredients.All( i => recipe.RequiredIngredients.Contains( i ) ) ).ToList();
	}

	/// <summary>
	/// Checks if a new ingredient can be added to the existing ingredients to make a recipe
	/// </summary>
	/// <param name="newIngredient"></param>
	/// <param name="existingIngredients"></param>
	/// <returns>True if the new ingredient can be added to the existing ingredients to make a recipe, false otherwise</returns>
	public bool CanAddIngredient( IngredientResource newIngredient, List<IngredientResource> existingIngredients )
	{
		var newIngredients = new List<IngredientResource>( existingIngredients ) { newIngredient };
		return GetRecipesIncludingIngredients( newIngredients ).Count > 0;
	}

	/// <summary>
	/// Gets a recipe that matches the given ingredients
	/// </summary>
	/// <param name="ingredients"></param>
	/// <returns>The recipe that matches the given ingredients, or null if no match is found</returns>
	public RecipeResource? GetRecipeFromIngredients( List<IngredientResource> ingredients )
	{
		foreach ( var recipe in LevelConfig.Instance.CookableRecipes )
		{
			// Check if ingredients exactly match the recipe requirements (same count and all ingredients present)
			if ( recipe.RequiredIngredients.Count == ingredients.Count && recipe.RequiredIngredients.All( ingredients.Contains ) )
			{
				return recipe;
			}
		}

		return null;
	}
}
