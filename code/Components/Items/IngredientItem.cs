#nullable enable

using System;
using Undercooked.Resources;

namespace Undercooked.Components;

public class IngredientItem : ItemBase
{
	[Property]
	[Description( "The resource of the ingredient" )]
	[Change]
	[Sync( SyncFlags.FromHost )]
	public IngredientResource? Resource { get; set; } = null;

	[Property]
	[FeatureEnabled( "Chop" )]
	[ReadOnly]
	public bool ChopFeatureEnabled => Resource?.ChopFeatureEnabled ?? false;

	[Property]
	[Feature( "Chop" )]
	[Range( 0f, 1f )]
	[Description( "The progress of the ingredient's chopping" )]
	[ReadOnly]
	public float ChopProgress { get; set; } = 0f;

	[Property]
	[FeatureEnabled( "Cook" )]
	[ReadOnly]
	public bool CookFeatureEnabled => Resource?.CookFeatureEnabled ?? false;

	[Property]
	[Feature( "Cook" )]
	[Description( "The progress of the ingredient's cooking" )]
	[ReadOnly]
	public float CookProgress { get; set; } = 0f;

	public bool Choppable => Resource?.ChopFeatureEnabled ?? false && ChopProgress < 1f;

	public bool Cookable => Resource?.CookFeatureEnabled ?? false && CookProgress < 1f;

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	public void OnChopped()
	{
		Resource = Resource?.ChoppedResource;
	}

	public void OnBurned()
	{
		Resource = Resource?.BurnedResource;
	}

	public void OnCooked()
	{
		Resource = Resource?.CookedResource;
	}

	protected virtual void OnResourceChanged( IngredientResource? oldResource, IngredientResource? newResource )
	{
		GameObject.Name = newResource?.Name ?? "Ingredient";
		ModelRenderer.Model = newResource?.Model;
		ModelCollider.Model = newResource?.Model;
		ChopProgress = 0f;
		CookProgress = 0f;
	}
}

