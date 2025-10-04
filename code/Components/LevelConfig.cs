#nullable enable

using Undercooked.Resources;

namespace Undercooked.Components;

public class WeightedRecipe
{
    public required RecipeResource Recipe;
    public int Weight { get; set; }

    public override string ToString()
    {
        return $"{Recipe}";
    }
}

public class LevelConfig : Component
{
    public static LevelConfig Instance { get; private set; } = null!;

    [Property]
    public List<RecipeResource> CookableRecipes { get; set; } = [];

    [Property]
    [Validate( nameof( IsCookable ), "At least one recipe is not in the cookable recipes", LogLevel.Error )]
    private List<WeightedRecipe> OrderableRecipes { get; set; } = [];

    public LevelConfig() : base()
    {
        Instance = this;
    }

    public bool IsCookable( List<WeightedRecipe> recipes )
    {
        foreach ( var recipe in recipes )
        {
            if ( !CookableRecipes.Contains( recipe.Recipe ) )
                return false;
        }

        return true;
    }

    public IEnumerable<RecipeResource> GetOrderableRecipes()
    {
        var cookable = new HashSet<RecipeResource>( CookableRecipes );
        foreach ( var wr in OrderableRecipes )
            if ( cookable.Contains( wr.Recipe ) )
                yield return wr.Recipe;
    }
}