#nullable enable

namespace Undercooked;

[AssetType( Name = "Recipe", Extension = "recipe", Category = "undercooked" )]
[Description( "A recipe for a dish" )]
public class RecipeResource : GameResource
{
	[Description( "The name of the recipe" )]
	public string Name { get; set; } = string.Empty;

	[Description( "The icon of the recipe" )]
	public Texture Icon { get; set; } = null!;

	[Description( "The ingredients that the recipe requires" )]
	public List<IngredientResource> RequiredIngredients { get; set; } = [];

	[Description( "The result of the recipe" )]
	public PrefabFile? Result { get; set; }

	[Description( "The full reward paid when the order is delivered immediately." )]
	public int BaseReward { get; set; } = 100;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "restaurant", width, height );
	}
}
