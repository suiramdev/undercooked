#nullable enable

using Undercooked.Resources;

namespace Undercooked.Components;

public class Order( RecipeResource recipe )
{
	public RecipeResource Recipe = recipe;

	public float PlacedAt = Time.Now;

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
			PlaceOrder( LevelConfig.Instance.GetRandomOrderableRecipe() );
			_lastOrderTime = Time.Now;
		}
	}

	public void PlaceOrder( RecipeResource recipe )
	{
		if ( Orders.Count >= LevelConfig.Instance.MaxOrders )
			return;

		Orders.Add( new Order( recipe ) );
	}
}