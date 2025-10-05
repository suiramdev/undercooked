#nullable enable

using Undercooked.Components.Interfaces;
using Undercooked.Resources;

namespace Undercooked.Components;

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

	public bool Empty => Ingredients.Count == 0;

	public bool CanDeposit( IPickable pickable, Player _ )
	{
		return pickable is IngredientItem ingredient
			&& ingredient.Resource is not null
			&& RecipeManager.Instance.CanAddIngredient( ingredient.Resource, Ingredients );
	}

	[Rpc.Host]
	public void Deposit( IPickable pickable, Player by )
	{
		if ( !CanDeposit( pickable, by ) ) return;
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
		}

		// Notify the ingredient that it was dropped on the plate
		pickable.OnDeposited( this, by );
	}

	public override bool CanBePickedUp( Player _ )
	{
		return true;
	}

	public override bool CanBeDepositedOn( IDepositable _, Player __ )
	{
		return true;
	}

	public IPickable? GetPickable() => null;

	public IPickable? TakePickable()
	{
		return null;
	}
}
