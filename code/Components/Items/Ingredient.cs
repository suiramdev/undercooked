#nullable enable

using Sandbox;
using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public class Ingredient : Item
{
	[Property]
	[Description( "The model of the ingredient" )]
	public required Model Model { get; set; }

	[Property]
	[Description( "The state of the ingredient" )]
	[ReadOnly]
	public IngredientState State { get; private set; } = IngredientState.Raw;

	[Property]
	[FeatureEnabled( "Chop" )]
	[Description( "Whether the ingredient can be chopped" )]
	public bool ChopFeatureEnabled { get; set; }

	[Property]
	[Feature( "Chop" )]
	[Description( "The model of the ingredient when it is chopped" )]
	public required Model ChoppedModel { get; set; }

	[Property]
	[Feature( "Chop" )]
	[Range( 0f, 1f )]
	[Description( "The progress of the ingredient's chopping" )]
	[ReadOnly]
	public float ChopProgress { get; set; } = 0f;

	[Property]
	[FeatureEnabled( "Cook" )]
	[Description( "Whether the ingredient can be cooked" )]
	public bool CookFeatureEnabled { get; set; }

	[Property]
	[Feature( "Cook" )]
	[Description( "The state required to cook the ingredient" )]
	public IngredientState CookableState { get; set; } = IngredientState.Chopped;

	[Property]
	[Feature( "Cook" )]
	[Description( "The model of the ingredient when it is cooked" )]
	public required Model CookedModel { get; set; }

	[Property]
	[Feature( "Cook" )]
	[Description( "The model of the ingredient when it is burnt" )]
	public required Model BurnedModel { get; set; }

	[Property]
	[Feature( "Cook" )]
	[Description( "The progress of the ingredient's cooking" )]
	[ReadOnly]
	public float CookProgress { get; set; } = 0f;

	public bool Choppable => ChopFeatureEnabled && State == IngredientState.Raw;

	public bool Cookable => CookFeatureEnabled && State >= CookableState;

	public void SetState( IngredientState state )
	{
		State = state;
		ModelRenderer.Model = state switch
		{
			IngredientState.Chopped => ChoppedModel,
			IngredientState.Cooked => CookedModel,
			IngredientState.Burnt => BurnedModel,
			_ => Model
		};
	}
}
