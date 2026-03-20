#nullable enable

using System;

namespace Undercooked;

[Icon( "table_restaurant" )]
[Title( "Plate Dispenser Station" )]
[Description( "Serves plates into a player's hands and can be restocked by depositing clean plates." )]
public sealed class PlateDispenserStation : Component, IInteractable, IDepositable
{
	private const string DEFAULT_PLATE_PREFAB_PATH = "prefabs/items/plate.prefab";

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required HighlightOutline HighlightOutline { get; set; }

	[Property]
	[Description( "Optional override for the plate prefab that gets served." )]
	public PrefabFile? PlatePrefab { get; set; }

	[Property]
	[Description( "How many plates this station starts with." )]
	public int StartingPlateCount { get; set; } = 4;

	[Property]
	[Description( "Maximum amount of plates this station can hold. Set to 0 or less for unlimited storage." )]
	public int MaxPlateCount { get; set; } = 4;

	[Property]
	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	public int PlateCount { get; private set; }

	public InteractionType InteractionType => InteractionType.Press;

	public InteractionType AlternateInteractionType => InteractionType.Press;

	protected override void OnAwake()
	{
		HighlightOutline.Enabled = false;
	}

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		PlateCount = ClampPlateCount( StartingPlateCount );
	}

	public string? GetInteractionText( Player by )
	{
		if ( by.StoredPickable is null )
			return CanServePlate() ? "Pickup" : null;

		return CanAccept( by.StoredPickable ) ? "Deposit" : null;
	}

	[Rpc.Host]
	public void TryInteract( Player by )
	{
		if ( by.StoredPickable is null )
		{
			TryServePlate( by );
			return;
		}

		by.TryTransfer( this );
	}

	public string? GetAlternateInteractionText( Player by ) => null;

	[Rpc.Host]
	public void TryAlternateInteract( Player by )
	{
		return;
	}

	public bool CanAccept( IPickable pickable )
	{
		return pickable is PlateItem plate
			&& plate.Ingredients.Count == 0
			&& HasRoomForMorePlates();
	}

	[Rpc.Host]
	public void TryDeposit( IPickable pickable )
	{
		if ( !CanAccept( pickable ) || pickable is not PlateItem plate )
			return;

		if ( AddPlates( 1 ) <= 0 )
			return;

		plate.OnDrop();
		plate.GameObject.Destroy();
	}

	public int AddPlates( int amount )
	{
		if ( amount <= 0 )
			return 0;

		if ( MaxPlateCount <= 0 )
		{
			PlateCount += amount;
			return amount;
		}

		int previousCount = PlateCount;
		PlateCount = Math.Min( PlateCount + amount, MaxPlateCount );
		return PlateCount - previousCount;
	}

	public int RemovePlates( int amount )
	{
		if ( amount <= 0 )
			return 0;

		int previousCount = PlateCount;
		PlateCount = Math.Max( PlateCount - amount, 0 );
		return previousCount - PlateCount;
	}

	private void TryServePlate( Player by )
	{
		if ( !CanServePlate() )
			return;

		var plate = SpawnPlate();
		if ( plate is null || !by.CanAccept( plate ) )
		{
			plate?.GameObject.Destroy();
			return;
		}

		if ( RemovePlates( 1 ) <= 0 )
		{
			plate.GameObject.Destroy();
			return;
		}

		by.TryDeposit( plate );
	}

	private bool CanServePlate()
	{
		return PlateCount > 0 && ResolvePlatePrefab() is not null;
	}

	private bool HasRoomForMorePlates()
	{
		return MaxPlateCount <= 0 || PlateCount < MaxPlateCount;
	}

	private int ClampPlateCount( int plateCount )
	{
		return MaxPlateCount > 0
			? Math.Clamp( plateCount, 0, MaxPlateCount )
			: Math.Max( plateCount, 0 );
	}

	private PrefabFile? ResolvePlatePrefab()
	{
		return PlatePrefab ?? ResourceLibrary.Get<PrefabFile>( DEFAULT_PLATE_PREFAB_PATH );
	}

	private PlateItem? SpawnPlate()
	{
		var platePrefab = ResolvePlatePrefab();
		if ( platePrefab is null )
			return null;

		var plateObject = GameObject.Clone( platePrefab );
		plateObject.WorldPosition = GameObject.WorldPosition;
		plateObject.WorldRotation = Rotation.Identity;

		var plate = plateObject.Components.Get<PlateItem>( true );
		if ( plate is null )
		{
			plateObject.Destroy();
			return null;
		}

		plateObject.NetworkSpawn();

		return plate;
	}
}
