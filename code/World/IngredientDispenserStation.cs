#nullable enable

namespace Undercooked;

[Icon( "kitchen" )]
[Title( "Ingredient Dispenser Station" )]
[Description( "Serves an unlimited amount of a configured ingredient directly into a player's hands." )]
public sealed class IngredientDispenserStation : Component, IInteractable
{
	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required HighlightOutline HighlightOutline { get; set; }

	[Property]
	[Description( "The ingredient resource this furniture serves." )]
	public IngredientResource? Ingredient { get; set; }

	public InteractionType InteractionType => InteractionType.Press;

	public InteractionType AlternateInteractionType => InteractionType.Press;

	protected override void OnAwake()
	{
		HighlightOutline.Enabled = false;
	}

	public string? GetInteractionText( Player by )
	{
		if ( Ingredient is null || by.StoredPickable is not null )
			return null;

		return "Pickup";
	}

	[Rpc.Host]
	public void TryInteract( Player by )
	{
		if ( Ingredient is null || by.StoredPickable is not null )
			return;

		var ingredient = SpawnIngredient();
		if ( ingredient is null || !by.CanAccept( ingredient ) )
		{
			ingredient?.GameObject.Destroy();
			return;
		}

		by.TryDeposit( ingredient );
	}

	public string? GetAlternateInteractionText( Player by ) => null;

	[Rpc.Host]
	public void TryAlternateInteract( Player by )
	{
		return;
	}

	private IngredientItem? SpawnIngredient()
	{
		if ( Ingredient is null )
			return null;

		var itemObject = new GameObject( Ingredient.Name );
		itemObject.WorldPosition = GameObject.WorldPosition;
		itemObject.WorldRotation = Rotation.Identity;

		var modelRenderer = itemObject.AddComponent<ModelRenderer>();
		var rigidbody = itemObject.AddComponent<Rigidbody>();
		var modelCollider = itemObject.AddComponent<ModelCollider>();
		var highlightOutline = itemObject.AddComponent<HighlightOutline>();

		modelRenderer.Model = Ingredient.Model;
		modelCollider.Model = Ingredient.Model;
		highlightOutline.Enabled = false;

		var ingredientItem = itemObject.Components.Create( TypeLibrary.GetType<IngredientItem>() ) as IngredientItem;
		if ( ingredientItem is null )
		{
			itemObject.Destroy();
			return null;
		}

		ingredientItem.ModelRenderer = modelRenderer;
		ingredientItem.Rigidbody = rigidbody;
		ingredientItem.ModelCollider = modelCollider;
		ingredientItem.HighlightOutline = highlightOutline;
		ingredientItem.Resource = Ingredient;

		CreateWorldPanel( itemObject, ingredientItem );

		itemObject.NetworkSpawn();

		return ingredientItem;
	}

	private static void CreateWorldPanel( GameObject itemObject, IngredientItem ingredientItem )
	{
		var worldObject = new GameObject( itemObject, name: "World" );
		worldObject.LocalPosition = Vector3.Up * 20f;

		var worldPanel = worldObject.AddComponent<WorldPanel>();
		worldPanel.PanelSize = new Vector2( 512f, 512f );
		worldPanel.RenderScale = 0.75f;
		worldPanel.LookAtCamera = true;
		worldPanel.InteractionRange = 1000f;
		worldPanel.HorizontalAlign = WorldPanel.HAlignment.Center;
		worldPanel.VerticalAlign = WorldPanel.VAlignment.Center;

		var ingredientPanel = worldObject.AddComponent<Undercooked.UI.IngredientWorldPanel.IngredientWorldPanel>();
		ingredientPanel.ingredient = ingredientItem;
	}
}
