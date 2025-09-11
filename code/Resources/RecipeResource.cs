#nullable enable

namespace Undercooked.Resources;

[AssetType( Name = "Recipe", Extension = "recipe", Category = "undercooked" )]
[Description( "A recipe for a dish" )]
public class RecipeResource : GameResource
{
	[Description( "The name of the recipe" )]
	public string Name { get; set; } = string.Empty;

	[Description( "The ingredients that the recipe requires" )]
	public List<IngredientResource> RequiredIngredients { get; set; } = [];

	[Description( "The result of the recipe" )]
	public PrefabFile? Result { get; set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "restaurant", width, height );
	}
}
