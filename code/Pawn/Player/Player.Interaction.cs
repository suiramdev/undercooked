#nullable enable

namespace Undercooked;

public partial class Player
{
	private bool _isChopping;
	private CuttingBoardStation? _choppingStation;
	[Property]
	[Header( "Interaction Settings" )]
	public float InteractRadius { get; set; } = 50f;

	[Property]
	[ReadOnly]
	public IInteractable? InteractableTarget { get; set; }

	private void HandleInteractionInput()
	{
		if ( IsProxy )
			return;

		InteractableTarget = GetInteractableTarget();

		UpdateChopping();

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
		bool altPressed = Input.Pressed( "AltInteract" );
		if (
			(InteractableTarget is null && altPressed) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Press && altPressed) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Hold && Input.Down( "AltInteract" )) ||
			(InteractableTarget?.AlternateInteractionType == InteractionType.Release && Input.Released( "AltInteract" ))
		)
		{
			if ( InteractableTarget is CuttingBoardStation cuttingBoard && altPressed )
			{
				StartChopping( cuttingBoard );
			}
			else
			{
				InteractAlternate();
			}
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

	private void StartChopping( CuttingBoardStation station )
	{
		_choppingStation = station;
		_isChopping = true;
		station.TryAlternateInteract( this );
	}

	private void UpdateChopping()
	{
		if ( !_isChopping )
			return;

		if ( _choppingStation is null )
		{
			_isChopping = false;
			return;
		}

		// Cancel chopping if the player starts moving or looks away from the cutting board
		bool hasMovementInput = Input.AnalogMove.Length > 0.01f;
		bool lostFocus = InteractableTarget?.GameObject != _choppingStation.GameObject;

		if ( hasMovementInput || lostFocus )
		{
			_isChopping = false;
			_choppingStation = null;
			return;
		}

		_choppingStation.TryAlternateInteract( this );
	}
}
