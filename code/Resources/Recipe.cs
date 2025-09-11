#nullable enable

using Undercooked.Components.Enums;

namespace Undercooked.Resources;

[GameResource( "Recipe", "recipe", "A recipe for a dish.", Icon = "menu_book" )]
public class Recipe : GameResource
{
	[Property]
	[Description( "The name of the recipe" )]
	public string Name { get; set; } = string.Empty;

	[Property]
	[Description( "The ingredients that the recipe requires" )]
	public List<IngredientType> RequiredIngredients { get; set; } = [];

	[Property]
	[Description( "The result of the recipe" )]
	public PrefabFile? Result { get; set; }
}