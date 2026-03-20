#nullable enable

using System;

namespace Undercooked;

public class Order( RecipeResource recipe )
{
	[Property]
	[ReadOnly]
	public readonly RecipeResource Recipe = recipe;

	[Property]
	[ReadOnly]
	public readonly float PlacedAt = Time.Now;

	public float GetRemainingTime( float timeout )
	{
		return Math.Max( 0f, timeout - (Time.Now - PlacedAt) );
	}

	public float GetRemainingFraction( float timeout )
	{
		if ( timeout <= 0f )
			return 0f;

		return GetRemainingTime( timeout ) / timeout;
	}

	public int CalculateReward( float timeout )
	{
		return Math.Max( 0, (int)MathF.Ceiling( Recipe.BaseReward * GetRemainingFraction( timeout ) ) );
	}

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
	[Sync( SyncFlags.FromHost )]
	public List<Order> Orders { get; set; } = [];

	private float _lastOrderTime = Time.Now;

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

		RemoveExpiredOrders();

		// Check if it's time to place a new order
		if ( Time.Now - _lastOrderTime >= 60f / LevelConfig.Instance.OrdersPerMinute )
		{
			PlaceOrder();
			_lastOrderTime = Time.Now;
		}
	}

	/// <summary>
	/// Remove orders whose time left has reached zero or below.
	/// </summary>
	private void RemoveExpiredOrders()
	{
		float timeout = LevelConfig.Instance.OrderTimeout;
		Orders.RemoveAll( o => (timeout - (Time.Now - o.PlacedAt)) <= 0f );
	}

	[Rpc.Host]
	public void PlaceOrder()
	{
		if ( Orders.Count >= LevelConfig.Instance.MaxOrders )
			return;

		Orders.Add( new Order( LevelConfig.Instance.GetRandomOrderableRecipe() ) );
	}

	/// <summary>
	/// Try to complete an order with the given recipe
	/// </summary>
	/// <param name="recipe">The recipe that was completed</param>
	[Rpc.Host]
	public void CompleteOrder( RecipeResource recipe )
	{
		TryCompleteOrder( recipe, out _ );
	}

	public bool TryCompleteOrder( RecipeResource recipe )
	{
		return TryCompleteOrder( recipe, out _ );
	}

	public bool TryCompleteOrder( RecipeResource recipe, out int reward )
	{
		reward = 0;

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

		reward = order.CalculateReward( LevelConfig.Instance.OrderTimeout );

		// Remove the order from the list
		Orders.Remove( order );
		Log.Info( $"Order completed: {recipe} for {reward}" );

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
