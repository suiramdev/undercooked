#nullable enable

using System;

namespace Undercooked;

public class IngredientItem : ItemBase
{
	private bool? _cachedCookableFallback = null;

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

	public bool Cookable
	{
		get
		{
			if ( Resource is not null )
			{
				_cachedCookableFallback = null; // Clear cache when Resource is available
				return Resource.CookFeatureEnabled && CookProgress < 1f;
			}

			// Fallback: if Resource is null, try to infer from GameObject name
			// This is a workaround for when Resource doesn't sync properly
			if ( _cachedCookableFallback.HasValue )
			{
				return _cachedCookableFallback.Value && CookProgress < 1f;
			}

			var name = GameObject.Name.ToLower();
			bool isCookable = name.Contains( "patty" ) && !name.Contains( "cooked" ) && !name.Contains( "burned" );
			_cachedCookableFallback = isCookable;

			return isCookable && CookProgress < 1f;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnStart()
	{
		base.OnStart();

		// Try to load Resource if it's null (workaround for sync issues)
		if ( Resource is null && !IsProxy )
		{
			// Try to infer resource path from GameObject name
			var name = GameObject.Name.ToLower().Replace( " ", "_" );
			var resourcePath = $"resources/{name}.ingr";

			var resource = ResourceLibrary.Get<IngredientResource>( resourcePath );
			if ( resource is not null )
			{
				Resource = resource;
			}
		}
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

