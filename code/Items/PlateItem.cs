#nullable enable

namespace Undercooked;

public sealed class PlateItem : ItemBase, IDepositable
{
	[Property]
	public required GameObject Socket { get; set; }

	[Property]
	[Description( "The ingredients that the plate contains" )]
	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	public List<IngredientResource> Ingredients { get; set; } = [];

	[Property]
	[Description( "The recipe that the plate contains" )]
	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	public RecipeResource? Recipe { get; private set; }

	private GameObject? _recipeResultObject;

	public bool CanAccept( IPickable pickable )
	{
		return pickable is IngredientItem ingredient
			&& ingredient.Resource is not null
			&& RecipeManager.Instance.CanAddIngredient( ingredient.Resource, Ingredients );
	}

	[Rpc.Host]
	public void TryDeposit( IPickable pickable )
	{
		if ( !CanAccept( pickable ) ) return;
		if ( pickable is not IngredientItem ingredient || ingredient.Resource is null ) return;

		Ingredients.Add( ingredient.Resource );
		ingredient.GameObject.Destroy();

		// Get the recipe that the plate can make
		Recipe = RecipeManager.Instance.GetRecipeFromIngredients( Ingredients );

		if ( Recipe != null )
		{
			_recipeResultObject?.Destroy();

			_recipeResultObject = GameObject.Clone( Recipe.Result );
			_recipeResultObject.SetParent( Socket );
			_recipeResultObject.LocalPosition = Vector3.Zero;
			_recipeResultObject.LocalRotation = Rotation.Identity;
			_recipeResultObject.NetworkSpawn();
		}

		pickable.OnDeposit( this );
	}
}
