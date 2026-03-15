#nullable enable

using System.Collections.Generic;

namespace Undercooked;

public partial class Player
{
	private IInteractable? _sustainedPrimaryTarget;
	private IInteractable? _sustainedAlternateTarget;

	[Property]
	[Header( "Interaction Settings" )]
	public float InteractRadius { get; set; } = 50f;

	[Property]
	[ReadOnly]
	public IInteractable? InteractableTarget { get; set; }

	private readonly HashSet<HighlightOutline> _activeHighlightOutlines = new();

	private void HandleInteractionInput()
	{
		if ( IsProxy )
			return;

		InteractableTarget = GetInteractableTarget();

		UpdateInteractableHighlights();

		UpdateSustainedPrimaryInteraction();
		UpdateSustainedAlternateInteraction();

		// Primary interaction ("Use" / "Interact" input) — one press to start when PressToStartCancelOnMove
		bool usePressed = Input.Pressed( "Interact" );
		bool altPressed = Input.Pressed( "AltInteract" );
		// Only Use (Interact) starts sustained actions like chopping; G (AltInteract) is for alternate e.g. take item back
		bool startSustainedPrimary = InteractableTarget?.InteractionType == InteractionType.PressToStartCancelOnMove && usePressed;
		if ( startSustainedPrimary )
		{
			StartSustainedPrimaryInteraction();
		}
		else if (
			(InteractableTarget is null && usePressed) ||
			(InteractableTarget?.InteractionType == InteractionType.Press && usePressed) ||
			(InteractableTarget?.InteractionType == InteractionType.Hold && Input.Down( "Interact" )) ||
			(InteractableTarget?.InteractionType == InteractionType.Release && Input.Released( "Interact" ))
		)
		{
			InteractPrimary();
		}
		else if ( InteractableTarget?.AlternateInteractionType == InteractionType.PressToStartCancelOnMove && altPressed )
		{
			StartSustainedAlternateInteraction();
		}
		else if (
			(InteractableTarget is null && altPressed) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Press && altPressed) ||
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
				x.GameObject != StoredPickable?.GameObject
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

	private void UpdateInteractableHighlights()
	{
		var nextActiveOutlines = new HashSet<HighlightOutline>();

		// If there is no current interactable target, just restore all previous highlights
		if ( InteractableTarget is not null )
		{
			var gameObject = InteractableTarget.GameObject;

			// Do not highlight the object we are currently holding
			if ( gameObject != StoredPickable?.GameObject )
			{
				// Resolve outline from the interactable component when it has RequireComponent(HighlightOutline),
				// so we get a reference even when the component is disabled (Components.Get may skip disabled).
				var outline = gameObject.Components.Get<HighlightOutline>( true );

				if ( outline is not null )
				{
					nextActiveOutlines.Add( outline );
					outline.Enabled = true;
				}
			}
		}

		// Disable outlines that are no longer active
		foreach ( var outline in _activeHighlightOutlines )
		{
			if ( !nextActiveOutlines.Contains( outline ) )
			{
				outline.Enabled = false;
			}
		}

		_activeHighlightOutlines.Clear();
		_activeHighlightOutlines.UnionWith( nextActiveOutlines );
	}

	protected void InteractPrimary()
	{
		if ( InteractableTarget is not null )
		{
			InteractableTarget.TryInteract( this );
			return;
		}

		Drop();
	}

	protected void InteractAlternate()
	{
		InteractableTarget?.TryAlternateInteract( this );
	}

	private void StartSustainedPrimaryInteraction()
	{
		if ( InteractableTarget is null )
			return;
		if ( _sustainedPrimaryTarget == InteractableTarget )
			return;
		_sustainedPrimaryTarget = InteractableTarget;
		_sustainedPrimaryTarget.TryInteract( this );
	}

	private void UpdateSustainedPrimaryInteraction()
	{
		if ( _sustainedPrimaryTarget is null )
			return;
		bool hasMovementInput = Input.AnalogMove.Length > 0.01f;
		bool lostFocus = InteractableTarget is not null && InteractableTarget.GameObject != _sustainedPrimaryTarget.GameObject;
		if ( hasMovementInput || lostFocus )
		{
			_sustainedPrimaryTarget = null;
			return;
		}
		_sustainedPrimaryTarget.TryInteract( this );
	}

	private void StartSustainedAlternateInteraction()
	{
		if ( InteractableTarget is null )
			return;
		// Only start on first press; if already sustaining this target, do not restart
		if ( _sustainedAlternateTarget == InteractableTarget )
			return;
		_sustainedAlternateTarget = InteractableTarget;
		_sustainedAlternateTarget.TryAlternateInteract( this );
	}

	private void UpdateSustainedAlternateInteraction()
	{
		if ( _sustainedAlternateTarget is null )
			return;

		// Stop when the cutting board no longer has a choppable ingredient (e.g. player picked it up)
		if ( _sustainedAlternateTarget is CuttingBoardStation station &&
			(station.StoredPickable is not IngredientItem ingredient || !ingredient.Choppable) )
		{
			_sustainedAlternateTarget = null;
			return;
		}

		// Stop when the player moves or looks at a different interactable (ignore momentary null target)
		bool hasMovementInput = Input.AnalogMove.Length > 0.01f;
		bool lostFocus = InteractableTarget is not null && InteractableTarget.GameObject != _sustainedAlternateTarget.GameObject;

		if ( hasMovementInput || lostFocus )
		{
			_sustainedAlternateTarget = null;
			return;
		}

		_sustainedAlternateTarget.TryAlternateInteract( this );
	}
}
