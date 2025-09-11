#nullable enable

using System;
using Undercooked.Components.Enums;
using Undercooked.Resources;

namespace Undercooked.Components;

public class IngredientItem : Item
{
	[Property]
	[Description( "The resource of the ingredient" )]
	public required IngredientResource Resource { get; set; }

	[Property]
	[FeatureEnabled( "Chop" )]
	[ReadOnly]
	public bool ChopFeatureEnabled => Resource.ChopFeatureEnabled;

	[Property]
	[Feature( "Chop" )]
	[Range( 0f, 1f )]
	[Description( "The progress of the ingredient's chopping" )]
	[ReadOnly]
	public float ChopProgress { get; set; } = 0f;

	[Property]
	[FeatureEnabled( "Cook" )]
	[ReadOnly]
	public bool CookFeatureEnabled => Resource.CookFeatureEnabled;

	[Property]
	[Feature( "Cook" )]
	[Description( "The progress of the ingredient's cooking" )]
	[ReadOnly]
	public float CookProgress { get; set; } = 0f;

	public bool Choppable => Resource.ChopFeatureEnabled && ChopProgress < 1f;

	public bool Cookable => Resource.CookFeatureEnabled && CookProgress < 1f;


	protected override void OnAwake()
	{
		base.OnAwake();
		Reset( Resource );
	}

	public void Reset( IngredientResource? resource = null )
	{
		Resource = resource ?? Resource;
		GameObject.Name = Resource.Name;
		ModelRenderer.Model = Resource.Model;
		ModelCollider.Model = Resource.Model;
		ChopProgress = 0f;
		CookProgress = 0f;
	}

	public void OnChopped()
	{
		Reset( Resource.ChoppedResource );
	}

	public void OnBurned()
	{
		Reset( Resource.BurnedResource );
	}

	public void OnCooked()
	{
		Reset( Resource.CookedResource );
	}


	public override bool CanPickup( Player player )
	{
		return true;
	}

	public override bool CanDrop( Player player )
	{
		return true;
	}
}
