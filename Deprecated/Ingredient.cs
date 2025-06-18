#nullable enable

using Sandbox;
using Undercooked.Resources.Enums;

namespace Undercooked.Resources;

[GameResource( "Ingredient", "ingr", "An ingredient in the game", Icon = "egg" )]
public class Ingredient : Item
{
	[ReadOnly]
	public new ItemType ItemType => ItemType.Ingredient;
	
	[Property]
	public bool IsCookable { get; set; } = true;

	[Property]
	[HideIf( nameof( IsCookable ), false )]
	public Ingredient? CookedVariant { get; set; }

	[Property]
	[HideIf( nameof( IsCookable ), false )]
	public Ingredient? BurnedVariant { get; set; }

	[Property]
	public bool IsCuttable { get; set; } = true;

	[Property]
	[HideIf( nameof( IsCuttable ), false )]
	public Ingredient? SlicedVariant { get; set; }
}
