#nullable enable

using Undercooked.Components.Interfaces;
using Undercooked.Components.Enums;

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

	[Property]
	[ReadOnly]
	public IInteractable? InteractableTarget { get; set; }

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		InteractableTarget = GetInteractableTarget();

		// Primary interaction ("drop" input)
		if (
			(InteractableTarget is null && Input.Pressed( "drop" )) ||
			(InteractableTarget?.InteractionType == InteractionType.Press && Input.Pressed( "drop" )) ||
			(InteractableTarget?.InteractionType == InteractionType.Hold && Input.Down( "drop" )) ||
			(InteractableTarget?.InteractionType == InteractionType.Release && Input.Released( "drop" ))
		)
		{
			InteractPrimary();
		}

		// Alternate interaction ("use" input)
		if (
			(InteractableTarget is null && Input.Pressed( "use" )) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Press && Input.Pressed( "use" )) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Hold && Input.Down( "use" )) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Release && Input.Released( "use" ))
		)
		{
			InteractAlternate();
		}
	}

	protected override void DrawGizmos()
	{
		if ( !EnableGizmos ) return;

		base.DrawGizmos();

		Gizmo.Draw.Color = GizmoColor;
		Gizmo.Draw.LineSphere( Vector3.Zero, InteractRadius );

		if ( InteractableTarget is not null )
		{
			var transform = new Transform( InteractableTarget.GameObject.WorldPosition, InteractableTarget.GameObject.WorldRotation, InteractableTarget.GameObject.WorldScale );
			using ( Gizmo.Scope( "Interactable Target", transform ) )
			{
				Gizmo.Transform = transform;
				Gizmo.Draw.Text( InteractableTarget.GameObject.Name, new Transform( Vector3.Zero, Rotation.Identity, Vector3.One ) );
			}
		}
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

	protected void InteractPrimary()
	{
		if ( !PlayerSlot.Empty && (InteractableTarget is null || InteractableTarget is ItemBase) )
		{
			// Attempt to drop the currently held item if no interactable target is found
			PlayerSlot.TryDrop();
			return;
		}

		InteractableTarget?.TryInteract( Player );
	}

	protected void InteractAlternate()
	{
		InteractableTarget?.TryAlternateInteract( Player );
	}
}
