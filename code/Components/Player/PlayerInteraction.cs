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

		if ( IsProxy )
			return;

		InteractableTarget = GetInteractableTarget();

		// Primary interaction ("Interact" input)
		if (
			(InteractableTarget is null && Input.Pressed( "Interact" )) ||
			(InteractableTarget?.InteractionType == InteractionType.Press && Input.Pressed( "Interact" )) ||
			(InteractableTarget?.InteractionType == InteractionType.Hold && Input.Down( "Interact" )) ||
			(InteractableTarget?.InteractionType == InteractionType.Release && Input.Released( "Interact" ))
		)
		{
			InteractPrimary();
		}

		// Alternate interaction ("AltInteract" input)
		if (
			(InteractableTarget is null && Input.Pressed( "AltInteract" )) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Press && Input.Pressed( "AltInteract" )) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Hold && Input.Down( "AltInteract" )) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Release && Input.Released( "AltInteract" ))
		)
		{
			InteractAlternate();
		}
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !Gizmo.IsSelected && !Gizmo.IsHovered )
			return;

		Gizmo.Draw.Color = Color.Green;
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
				x.GameObject != PlayerSlot.StoredPickable?.GameObject
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
		if ( InteractableTarget is not null )
		{
			InteractableTarget.TryInteract( Player );
			return;
		}

		PlayerSlot.Drop();
	}

	protected void InteractAlternate()
	{
		InteractableTarget?.TryAlternateInteract( Player );
	}
}
