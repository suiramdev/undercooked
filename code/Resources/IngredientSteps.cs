#nullable enable

using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Undercooked;

/// <summary>
/// Resolves how to obtain a required ingredient: which raw resource to use and which actions to perform, in order.
/// </summary>
public static class IngredientSteps
{
	private static IReadOnlyList<IngredientResource> AllIngredients =>
		ResourceLibrary.GetAll<IngredientResource>().ToList();

	/// <summary>
	/// Gets the preparation steps for a required ingredient: the raw resource and the ordered list of actions.
	/// </summary>
	public static IngredientPreparation GetPreparationFor( IngredientResource target )
	{
		var actions = new List<IngredientActionType>();

		IngredientResource? raw = FindProducingResource( target );
		if ( raw is null )
		{
			// No transformation: required ingredient is used as-is (e.g. bun).
			return new IngredientPreparation
			{
				RawIngredient = target,
				Actions = actions
			};
		}

		if ( raw.ChopFeatureEnabled && raw.ChoppedResource == target )
			actions.Add( IngredientActionType.Chop );
		if ( raw.CookFeatureEnabled && raw.CookedResource == target )
			actions.Add( IngredientActionType.Cook );

		return new IngredientPreparation
		{
			RawIngredient = raw,
			Actions = actions
		};
	}

	/// <summary>
	/// Returns the icon for an action type (e.g. for use in Image src).
	/// </summary>
	public static Texture GetActionIcon( IngredientActionType action )
	{
		return action switch
		{
			IngredientActionType.Chop => Texture.Load( "/ui/table_knife_black_64.png" ),
			IngredientActionType.Cook => Texture.Load( "/ui/frying_pan_black_64.png" ),
			_ => Texture.Invalid
		};
	}

	private static IngredientResource? FindProducingResource( IngredientResource target )
	{
		return AllIngredients.FirstOrDefault( x =>
			(x.ChopFeatureEnabled && x.ChoppedResource == target) ||
			(x.CookFeatureEnabled && x.CookedResource == target) );
	}
}
