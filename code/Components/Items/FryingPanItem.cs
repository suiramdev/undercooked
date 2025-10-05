#nullable enable

using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public class FryingPanItem : ItemBase, IDepositable, ITransferable
{
	[Property]
	[Description( "The attachment object of the utensil" )]
	public required GameObject Socket { get; set; }

	[Property]
	[Description( "The ingredients that the utensil contains" )]
	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	public IngredientItem? Ingredient { get; set; }

	public bool Empty => Ingredient is null;

	[Rpc.Host]
	public void Deposit( IPickable pickable, Player by )
	{
		if ( pickable is not IngredientItem ingredient || !ingredient.Cookable ) return;

		Ingredient = ingredient;
		ingredient.GameObject.SetParent( Socket );
		ingredient.GameObject.LocalPosition = Vector3.Zero;
		ingredient.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = ingredient.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody is not null ) rigidbody.Enabled = false;

		Collider? collider = ingredient.GameObject.GetComponent<Collider>( true );
		if ( collider is not null ) collider.Enabled = false;

		// Notify the ingredient that it was dropped on the pan
		pickable.OnDeposited( this, by );
	}

	[Rpc.Host]
	public void TransferPickable( IDepositable depositable, Player by )
	{
		if ( Ingredient is not null )
		{
			depositable.Deposit( Ingredient, by );
		}
	}

	public override bool CanBePickedUp( Player by )
	{
		return true;
	}

	public override bool CanBeDepositedOn( IDepositable depositable, Player by )
	{
		return true;
	}

	public IPickable? GetPickable() => Ingredient;

	public IPickable? TakePickable()
	{
		var temp = Ingredient;
		Ingredient = null;

		// If the ingredient is embedded, unembed it
		if ( temp is not null ) temp.Depositable = null;

		return temp;
	}
}
