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
	public IngredientItem? Ingredient { get; set; }

	public bool Empty => Ingredient is null;

	public bool TryDeposit( IPickable pickable, Player by )
	{
		if ( pickable is not IngredientItem ingredient || !ingredient.Cookable ) return false;

		Ingredient = ingredient;
		ingredient.GameObject.SetParent( Socket );
		ingredient.GameObject.LocalPosition = Vector3.Zero;
		ingredient.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = ingredient.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody is not null ) rigidbody.Enabled = false;

		Collider? collider = ingredient.GameObject.GetComponent<Collider>( true );
		if ( collider is not null ) collider.Enabled = false;

		// Notify the ingredient that it was dropped on the pan
		pickable.OnDroppedOn( this, by );

		return true;
	}

	public bool TryTransfer( IDepositable depositable, Player by )
	{
		return Ingredient is not null && depositable.TryDeposit( Ingredient, by );
	}

	public override bool CanBePickedUp( Player by )
	{
		return true;
	}

	public override bool CanBeDroppedOn( IDepositable depositable, Player by )
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
