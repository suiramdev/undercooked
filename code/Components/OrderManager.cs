#nullable enable

using Undercooked.Resources;

namespace Undercooked.Components;

public class Order( RecipeResource recipe )
{
	[Property]
	[ReadOnly]
	public readonly RecipeResource Recipe = recipe;

	[Property]
	[ReadOnly]
	public readonly float PlacedAt = Time.Now;

	public override string ToString()
	{
		return $"{Recipe}";
	}
}

public class OrderManager : Component
{
	public static OrderManager Instance { get; private set; } = null!;

	[Property]
	[ReadOnly]
	public List<Order> Orders { get; set; } = [];

	private float _lastOrderTime = 0f;

	public OrderManager() : base()
	{
		Instance = this;
	}

	protected override void OnStart()
	{
		base.OnStart();
		// Calculate the interval between orders based on OrdersPerMinute
		_lastOrderTime = Time.Now;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Check if it's time to place a new order
		if ( Time.Now - _lastOrderTime >= 60f / LevelConfig.Instance.OrdersPerMinute )
		{
			RecipeResource recipe = LevelConfig.Instance.GetRandomOrderableRecipe();
			PlaceOrder( recipe );
			_lastOrderTime = Time.Now;
		}
	}

	public void PlaceOrder( RecipeResource recipe )
	{
		if ( Orders.Count >= LevelConfig.Instance.MaxOrders )
			return;

		Orders.Add( new Order( recipe ) );
	}

	/// <summary>
	/// Try to complete an order with the given recipe
	/// </summary>
	/// <param name="recipe">The recipe that was completed</param>
	/// <returns>True if an order was completed, false otherwise</returns>
	public bool TryCompleteOrder( RecipeResource recipe )
	{
		// Find the oldest order that matches the recipe
		var order = Orders
			.Where( o => o.Recipe == recipe )
			.OrderBy( o => o.PlacedAt )
			.FirstOrDefault();

		if ( order is null )
		{
			Log.Warning( $"No matching order found for recipe: {recipe}" );
			return false;
		}

		// Remove the order from the list
		Orders.Remove( order );
		Log.Info( $"Order completed: {recipe}" );

		// TODO: Add score/points logic here
		// TODO: Add sound effects or visual feedback

		return true;
	}

	/// <summary>
	/// Check if there's a pending order for the given recipe
	/// </summary>
	public bool HasOrderFor( RecipeResource recipe )
	{
		return Orders.Any( o => o.Recipe == recipe );
	}
}