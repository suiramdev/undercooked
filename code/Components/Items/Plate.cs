#nullable enable

using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public abstract class Plate : BaseUtensil
{
	[Property]
	[Description("The anchor point of the plate")]
	public GameObject? ItemAnchorPoint { get; set; }

	[Property]
	[Description("The ingredients that the plate contains")]
	[ReadOnly]
	public List<Ingredient> Ingredients { get; set; } = [];

	public override bool CanAccept(IPickable pickable, Player player)
	{
		return pickable is Ingredient ingredient && ingredient.Servable;
	}

	public override bool CanWithdraw(Player player)
	{
		return false;
	}

	public override void OnDeposit(IPickable pickable, Player player)
	{
		if (pickable is not Ingredient ingredient)
		{
			return;
		}

		Ingredients.Add(ingredient);
		ingredient.GameObject.SetParent(ItemAnchorPoint ?? GameObject);
		ingredient.GameObject.LocalPosition = Vector3.Zero;
		ingredient.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = ingredient.GameObject.GetComponent<Rigidbody>(true);
		if (rigidbody != null)
		{
			rigidbody.Enabled = false;
		}

		Collider? collider = ingredient.GameObject.GetComponent<Collider>(true);
		if (collider != null)
		{
			collider.Enabled = false;
		}
	}

	public override void OnWithdraw(IPickable pickable, Player player)
	{
		// TODO: Implement
	}

	public override IPickable? GetStoredPickable()
	{
		return Ingredients.FirstOrDefault();
	}
}
