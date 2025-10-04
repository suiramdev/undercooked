#nullable enable

using System;
using Undercooked.Resources;

namespace Undercooked.Components;

public class Order
{
	public required RecipeResource Recipe;

	public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}

public class OrderManager : Component
{
	public static OrderManager Instance { get; private set; } = null!;

	[Property]
	[ReadOnly]
	public List<Order> PendingOrders { get; set; } = [];

	public OrderManager() : base()
	{
		Instance = this;
	}
}