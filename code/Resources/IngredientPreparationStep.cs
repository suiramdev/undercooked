#nullable enable

using System.Collections.Generic;
using Sandbox;

namespace Undercooked;

/// <summary>
/// Type of action the player must perform on an ingredient to reach the required state.
/// </summary>
public enum IngredientActionType
{
	Chop,
	Cook
}

/// <summary>
/// Result of resolving how to obtain a required ingredient: the raw resource and the ordered actions to perform.
/// </summary>
public class IngredientPreparation
{
	/// <summary>
	/// The base/raw ingredient the player must obtain first.
	/// </summary>
	public IngredientResource RawIngredient { get; init; } = null!;

	/// <summary>
	/// Actions to perform on the raw ingredient, in order, to achieve the required ingredient.
	/// Empty when no transformation is needed (e.g. bun).
	/// </summary>
	public IReadOnlyList<IngredientActionType> Actions { get; init; } = [];
}
