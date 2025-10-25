
#nullable enable

namespace Undercooked;

[AssetType( Name = "Ingredient", Extension = "ingr", Category = "undercooked" )]
public class IngredientResource : GameResource
{
	[Description( "The name of the ingredient" )]
	public string Name { get; set; } = string.Empty;

	[Description( "The model of the ingredient" )]
	public Model Model { get; set; } = null!;

	[FeatureEnabled( "Chop" )]
	[Description( "Whether the ingredient can be chopped" )]
	public bool ChopFeatureEnabled { get; set; }

	[Feature( "Chop" )]
	[Description( "The ingredient resource that is the result of chopping the ingredient" )]
	public IngredientResource ChoppedResource { get; set; } = null!;

	[FeatureEnabled( "Cook" )]
	[Description( "Whether the ingredient can be cooked" )]
	public bool CookFeatureEnabled { get; set; }

	[Feature( "Cook" )]
	[Description( "The ingredient resource that is the result of cooking the ingredient" )]
	public IngredientResource CookedResource { get; set; } = null!;

	[Feature( "Cook" )]
	[Description( "The ingredient resource that is the result of burning the ingredient" )]
	public IngredientResource BurnedResource { get; set; } = null!;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "egg", width, height );
	}
}
