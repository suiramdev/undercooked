#nullable enable

using System.Diagnostics;
using System.Net.Http.Headers;
using Sandbox;
using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

[Icon( "touch_app" )]
public class PlayerUse : Component
{
	[Property]
	public float UseRadius { get; set; } = 50f;

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required Player Player { get; set; }

	[Property]
	[Feature( "Debug" )]
	public bool ShowGizmos { get; set; } = true;

	[Property]
	[Feature( "Debug" )]
	[HideIf( nameof( ShowGizmos ), false )]
	public Color GizmoColor { get; set; } = Color.Green;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Input.Pressed( "use" ) )
		{
			TryInteract( UseType.Press );
		}
		else if ( Input.Down( "use" ) )
		{
			TryInteract( UseType.Hold );
		}
		else if ( Input.Released( "use" ) )
		{
			TryInteract( UseType.Release );
		}
	}

	protected override void DrawGizmos()
	{
		if ( !ShowGizmos )
		{
			return;
		}

		base.DrawGizmos();

		Gizmo.Draw.Color = GizmoColor;
		Gizmo.Draw.LineSphere( Vector3.Zero, UseRadius );
	}

	public void TryInteract( UseType useType )
	{
		IEnumerable<SceneTraceResult> traceResults = Scene.Trace.Sphere( UseRadius, WorldPosition, WorldPosition )
			.RunAll();

		// Order trace results by the most facing direction
		// This is to ensure that the player uses the item that is most facing them
		IEnumerable<SceneTraceResult> orderedTraceResults = traceResults.OrderByDescending( traceResult =>
		{
			Vector3 toObject = (traceResult.GameObject.WorldPosition - WorldPosition).Normal;
			Vector3 facing = WorldRotation.Forward;
			var facingScore = toObject.Dot( facing );
			return facingScore;
		} );

		foreach ( SceneTraceResult traceResult in orderedTraceResults )
		{
			if ( traceResult.Hit )
			{
				IUsable usable = traceResult.GameObject.GetComponent<IUsable>();
				if ( usable != null && TryUse( usable, useType ) )
				{
					return;
				}
			}
		}
	}

	public bool TryUse( IUsable usable, UseType useType )
	{
		if ( useType != usable.UseType )
		{
			return false;
		}

		if ( usable.CanUse( Player ) )
		{
			usable.OnUse( Player );
			return true;
		}

		return false;
	}
}
