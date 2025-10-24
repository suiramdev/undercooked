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

	public bool CanAccept( IPickable pickable )
	{
		return pickable is IngredientItem ingredient && ingredient.Cookable;
	}

	[Rpc.Host]
	public void TryDeposit( IPickable pickable )
	{
		if ( !CanAccept( pickable ) ) return;
		if ( pickable is not IngredientItem ingredient ) return;

		Ingredient = ingredient;
		ingredient.GameObject.SetParent( Socket );
		ingredient.GameObject.LocalPosition = Vector3.Zero;
		ingredient.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = ingredient.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody is not null ) rigidbody.Enabled = false;

		Collider? collider = ingredient.GameObject.GetComponent<Collider>( true );
		if ( collider is not null ) collider.Enabled = false;

		// Notify the ingredient that it was dropped on the pan
		pickable.OnDeposit( this );
	}

	[Rpc.Host]
	public void TryTransfer( IDepositable depositable )
	{
		Log.Info( "Trying to transfer ingredient to depositable" );
		if ( Ingredient is not null )
		{
			depositable.TryDeposit( Ingredient );
		}
	}
}
