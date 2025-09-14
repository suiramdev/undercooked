#nullable enable

using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

[Icon( "front_hand" )]
public class PlayerInteraction : Component
{
	[Property]
	public float InteractRadius { get; set; } = 50f;

	[Property]
	[FeatureEnabled( "Gizmos" )]
	public bool EnableGizmos { get; set; } = true;

	[Property]
	[Feature( "Gizmos" )]
	public Color GizmoColor { get; set; } = Color.Green;

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required Player Player { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required PlayerSlot PlayerSlot { get; set; }

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Input.Pressed( "drop" ) )
		{
			TryInteractPrimary();
		}

		if ( Input.Pressed( "use" ) )
		{
			TryInteractAlternate();
		}
	}

	protected override void DrawGizmos()
	{
		if ( !EnableGizmos ) return;

		base.DrawGizmos();

		Gizmo.Draw.Color = GizmoColor;
		Gizmo.Draw.LineSphere( Vector3.Zero, InteractRadius );
	}

	private IInteractable? GetInteractableTarget()
	{
		var traceResults = Scene.Trace.Sphere( InteractRadius, WorldPosition, WorldPosition )
			.RunAll();

		return traceResults
			.Select( x => new
			{
				x.GameObject,
				Interactable = x.GameObject.Components.Get<IInteractable>()
			} )
			.Where( x =>
				x.Interactable is not null &&
				// We don't want to interact with the object we are holding
				x.GameObject != PlayerSlot.GetPickable()?.GameObject
			)
			.OrderByDescending( x =>
			{
				// Combine dot product (facing) and distance into a single score for ordering
				Vector3 toObject = (x.GameObject.WorldPosition - WorldPosition).Normal;
				Vector3 facing = WorldRotation.Forward;
				float dot = toObject.Dot( facing );
				float distance = (x.GameObject.WorldPosition - WorldPosition).Length;
				// Higher dot (more in front) and closer distance = higher score
				return dot + (1.0f / (distance + 0.01f));
			} )
			.Select( x => x.Interactable )
			.FirstOrDefault();
	}

	protected bool TryInteractPrimary()
	{
		var target = GetInteractableTarget();
		if ( target is null || (target is ItemBase && !PlayerSlot.Empty) )
		{
			// Attempt to drop the currently held item if no interactable target is found
			PlayerSlot.TryDrop();
			return false;
		}

		return target.TryInteract( Player );
	}

	protected bool TryInteractAlternate()
	{
		var target = GetInteractableTarget();
		if ( target is null ) return false;

		return target.TryAlternateInteract( Player );
	}
}
