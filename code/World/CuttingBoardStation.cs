#nullable enable

using System;

namespace Undercooked;

public class CuttingBoardStation : StationBase
{
	[Property]
	[Description( "How fast the chopping progress increases per second" )]
	public float ChopSpeed { get; set; } = 0.2f;

	private TimeSince _lastChopTime = 0f;
	private const float CHOP_COOLDOWN = 0.1f;

	/// <summary>G = chop when there is a choppable ingredient; Use = deposit / take back.</summary>
	public override InteractionType AlternateInteractionType =>
		StoredPickable is IngredientItem i && i.Choppable
			? global::Undercooked.InteractionType.PressToStartCancelOnMove
			: global::Undercooked.InteractionType.Press;

	// Use = deposit when holding, take back when board has item and hands are empty
	public override string? GetInteractionText( Player by )
	{
		if ( by.StoredPickable is not null && CanAccept( by.StoredPickable ) )
			return "Deposit";
		if ( by.StoredPickable is null && StoredPickable is not null && by.CanAccept( StoredPickable ) )
			return "Take back";
		return null;
	}

	// G = chop only; shows nothing when there is nothing to chop
	public override string? GetAlternateInteractionText( Player by )
	{
		if ( StoredPickable is IngredientItem ingredient && ingredient.Choppable && by.StoredPickable is null )
			return "Chop";
		return null;
	}

	[Rpc.Host]
	public override void TryInteract( Player by )
	{
		// Take item back when hands are empty
		if ( by.StoredPickable is null && StoredPickable is not null && by.CanAccept( StoredPickable ) )
		{
			var pickable = StoredPickable;
			StoredPickable = null;
			by.TryDeposit( pickable );
			return;
		}
		// Deposit when holding something
		if ( by.StoredPickable is not null )
			base.TryInteract( by );
	}

	[Rpc.Host]
	public override void TryAlternateInteract( Player by )
	{
		// G = chop only
		if ( StoredPickable is IngredientItem ingredient && ingredient.Choppable && by.StoredPickable is null )
			DoChop( by );
	}

	private void DoChop( Player by )
	{
		if ( StoredPickable is not IngredientItem ingredient )
			return;
		if ( !ingredient.Choppable || _lastChopTime < CHOP_COOLDOWN )
			return;
		ingredient.ChopProgress = MathF.Min( ingredient.ChopProgress + ChopSpeed * CHOP_COOLDOWN, 1f );
		_lastChopTime = 0f;
		if ( ingredient.ChopProgress >= 1f )
			ingredient.OnChopped();
	}
}
