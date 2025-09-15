#nullable enable

using System;
using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public class CuttingBoardStation : StationBase<IPickable>
{
	[Property]
	[Description( "The type of use for the chop counter" )]
	public UseType UseType { get; set; } = UseType.Hold;

	[Property]
	[Description( "How fast the chopping progress increases per second" )]
	public float ChopSpeed { get; set; } = 0.2f;

	private TimeSince _lastChopTime = 0f;
	private const float CHOP_COOLDOWN = 0.1f;

	public override bool TryAlternateInteract( Player by )
	{
		if ( StoredPickable is null || StoredPickable is not IngredientItem ingredient ) return false;

		if ( !ingredient.Choppable || _lastChopTime < CHOP_COOLDOWN )
		{
			return false;
		}

		ingredient.ChopProgress = MathF.Min( ingredient.ChopProgress + ChopSpeed * CHOP_COOLDOWN, 1f );
		_lastChopTime = 0f;

		if ( ingredient.ChopProgress >= 1f )
		{
			ingredient.OnChopped();
		}

		return true;
	}
}
