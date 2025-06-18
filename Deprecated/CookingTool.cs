using System;
using Sandbox;
using Undercooked.Resources.Enums;

namespace Undercooked.Resources;

[GameResource( "Cooking Tool", "cooktool", "A cooking tool in the game", Icon = "blender" )]
public class CookingTool : Item
{
	[ReadOnly]
	public new ItemType ItemType => ItemType.CookingTool;
	
	[Property]
	public int MaxIngredients { get; set; } = 1;
	
	[Property]
	[Group( "Content" )]
	public Ingredient[] Ingredients { get; set; } = [];
}
