#nullable enable

using System;
using Undercooked.Resources;

namespace Undercooked.Components;

public class WeightedRecipe
{
    [Property]
    public required RecipeResource Recipe { get; set; }

    [Property]
    [Range( 1, 100 )]
    public int Weight { get; set; } = 1;
}

public class LevelConfig : Component
{
    public static LevelConfig Instance { get; private set; } = null!;

    [Property]
    public List<RecipeResource> CookableRecipes { get; set; } = [];

    [Property]
    [Validate( nameof( IsCookable ), "At least one recipe is not in the cookable recipes", LogLevel.Error )]
    [InlineEditor]
    public List<WeightedRecipe> OrderableRecipes { get; set; } = [];

    [Property]
    [Description( "Orders per minute baseline" )]
    public float OrdersPerMinute { get; set; } = 6.0f;

    [Property]
    [Description( "Maximum number of orders that can be pending" )]
    public int MaxOrders { get; set; } = 5;

    [Property]
    [Description( "Average time (sec) before customer leaves" )]
    public float OrderTimeout { get; set; } = 45.0f;

    public LevelConfig() : base()
    {
        Instance = this;
    }

    private bool IsCookable( List<WeightedRecipe> recipes )
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

    public RecipeResource GetRandomOrderableRecipe()
    {
        if ( OrderableRecipes.Count == 0 )
            throw new InvalidOperationException( "No orderable recipes available." );

        float totalWeight = OrderableRecipes.Sum( wr => wr.Weight );
        if ( totalWeight <= 0 )
            throw new InvalidOperationException( "Total weight of orderable recipes must be greater than zero." );

        Log.Info( $"Total weight of orderable recipes: {totalWeight}" );

        // Generate a random float between 0 and the total weight of all orderable recipes.
        float rand = Game.Random.Float( 0, totalWeight );
        // This variable will keep a running total of the weights as we iterate.
        float cumulative = 0f;

        // Iterate through each weighted recipe in the list.
        foreach ( var wr in OrderableRecipes )
        {
            // Add the current recipe's weight to the cumulative total.
            cumulative += wr.Weight;
            // If the random value falls within the current cumulative range,
            // select and return this recipe. This ensures recipes with higher
            // weights are more likely to be chosen.
            if ( rand <= cumulative )
            {
                Log.Info( $"Selected recipe {wr.Recipe} for {rand} / {totalWeight}" );
                return wr.Recipe;
            }
        }

        Log.Info( $"No orderable recipe found for {rand} / {totalWeight}" );

        // Fallback, should not happen, but return the last recipe
        return OrderableRecipes.Last().Recipe;
    }
}