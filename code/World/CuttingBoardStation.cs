#nullable enable

using System;

namespace Undercooked;

public class CuttingBoardStation : StationBase
{
	[Property]
	[Description( "The type of use for the chop counter" )]
	public override InteractionType AlternateInteractionType { get; set; } = InteractionType.Hold;

	[Property]
	[Description( "How fast the chopping progress increases per second" )]
	public float ChopSpeed { get; set; } = 0.2f;

	public override string? AlternateInteractionText => "Chop";

	private TimeSince _lastChopTime = 0f;
	private const float CHOP_COOLDOWN = 0.1f;

	public override bool CanAlternateInteract( Player by )
	{
		return StoredPickable is not null && StoredPickable is IngredientItem ingredient && ingredient.Choppable;
	}

	[Rpc.Host]
	public override void TryAlternateInteract( Player by )
	{
		if ( !CanAlternateInteract( by ) ) return;
		if ( StoredPickable is not IngredientItem ingredient ) return;

		if ( !ingredient.Choppable || _lastChopTime < CHOP_COOLDOWN )
		{
			return;
		}

		ingredient.ChopProgress = MathF.Min( ingredient.ChopProgress + ChopSpeed * CHOP_COOLDOWN, 1f );
		_lastChopTime = 0f;

		if ( ingredient.ChopProgress >= 1f )
		{
			ingredient.OnChopped();
		}
	}
}
