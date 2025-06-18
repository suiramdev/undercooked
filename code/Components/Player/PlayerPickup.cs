#nullable enable

using Sandbox;
using Sandbox.Citizen;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

[Icon( "front_hand" )]
public class PlayerPickup : Component
{
	[Property]
	public float PickupRadius { get; set; } = 50f;

	[Property]
	[Feature( "Components" )]
	[RequireComponent]
	public required Player Player { get; set; }

	[Property]
	[ReadOnly]
	public IPickable? HeldPickable { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public CitizenAnimationHelper? CitizenAnimationHelper { get; set; }

	[Property]
	[Group( "Components" )]
	public SkinnedModelRenderer? SkinnedModelRenderer { get; set; }

	[Property]
	[FeatureEnabled( "Gizmos" )]
	public bool Gizmos { get; set; } = true;

	[Property]
	[Feature( "Gizmos" )]
	public Color GizmoColor { get; set; } = Color.Green;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Input.Pressed( "drop" ) )
		{
			TryInteract();
		}
	}

	protected override void DrawGizmos()
	{
		if ( !Gizmos )
		{
			return;
		}

		base.DrawGizmos();

		Gizmo.Draw.Color = GizmoColor;
		Gizmo.Draw.LineSphere( Vector3.Zero, PickupRadius );
	}

	protected void TryInteract()
	{
		IEnumerable<SceneTraceResult> traceResults = Scene.Trace.Sphere( PickupRadius, WorldPosition, WorldPosition )
			.RunAll();

		// Order trace results by the most facing direction
		// This is to ensure that the player picks up the object that is most facing them
		IEnumerable<SceneTraceResult> orderedTraceResults = traceResults.OrderByDescending( traceResult =>
		{
			Vector3 toObject = (traceResult.GameObject.WorldPosition - WorldPosition).Normal;
			Vector3 facing = WorldRotation.Forward;
			var facingScore = toObject.Dot( facing );
			return facingScore;
		} );

		foreach ( SceneTraceResult traceResult in orderedTraceResults )
		{
			if ( !traceResult.Hit ) continue;

			GameObject hitObject = traceResult.GameObject;

			var pickable = hitObject.GetComponent<IPickable>();
			if ( pickable != null && TryPickup( pickable ) )
			{
				return;
			}

			// Try to deposit a pickable if holding one
			var storage = hitObject.GetComponent<IDepositable>();
			if ( storage != null && (TryDeposit( storage ) || TryWithdraw( storage )) )
			{
				return;
			}
		}

		// Drop the held pickable if no interaction is possible
		TryDrop();
	}

	/// <summary>
	/// Try to deposit an object
	/// </summary>
	/// <param name="depositable">The storage object to deposit the object to</param>
	/// <returns>True if the object was deposited, false otherwise</returns>
	public bool TryDeposit( IDepositable depositable )
	{
		if ( HeldPickable == null )
		{
			return false;
		}

		if ( depositable.CanAccept( HeldPickable, Player ) )
		{
			// Remove the object from the player's hand
			HeldPickable.GameObject.SetParent( null );

			// Re-enable the object's physics
			Rigidbody? rigidbody = HeldPickable.GameObject.GetComponent<Rigidbody>( true );
			if ( rigidbody != null )
			{
				rigidbody.Enabled = true;
			}

			Collider? collider = HeldPickable.GameObject.GetComponent<Collider>( true );
			if ( collider != null )
			{
				collider.Enabled = true;
			}

			if ( CitizenAnimationHelper != null )
			{
				CitizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			}

			depositable.OnDeposit( HeldPickable, Player );

			HeldPickable = null;

			return true;
		}

		return false;
	}

	/// <summary>
	/// Try to withdraw an object
	/// </summary>
	/// <param name="depositable">The storage object to withdraw the object from</param>
	/// <returns>True if the object was withdrawn, false otherwise</returns>
	public bool TryWithdraw( IDepositable depositable )
	{
		if ( HeldPickable != null )
		{
			return false;
		}

		if ( depositable.CanWithdraw( Player ) )
		{
			IPickable? pickable = depositable.GetStoredPickable();
			if ( pickable != null )
			{
				HeldPickable = pickable;

				// Set the object's parent to the attachment object
				GameObject attachmentObject = SkinnedModelRenderer?.GetAttachmentObject( pickable.AttachmentBone ) ?? pickable.GameObject;
				HeldPickable.GameObject.SetParent( attachmentObject );
				HeldPickable.GameObject.LocalPosition = HeldPickable.AttachmentOffset;
				HeldPickable.GameObject.LocalRotation = HeldPickable.AttachmentRotation;

				// Disable the object's physics
				HeldPickable.GameObject.GetComponent<Rigidbody>( true ).Enabled = false;
				HeldPickable.GameObject.GetComponent<Collider>( true ).Enabled = false;

				// Set the player's hold type
				if ( CitizenAnimationHelper != null )
				{
					CitizenAnimationHelper.HoldType = HeldPickable.HoldType;
				}

				depositable.OnWithdraw( pickable, Player );

				return true;
			}
		}

		return false;
	}
	/// <summary>
	/// Try to pick up an object
	/// </summary>
	/// <param name="pickable">The object to pick up</param>
	/// <returns>True if the object was picked up, false otherwise</returns>
	public bool TryPickup( IPickable pickable )
	{
		if ( HeldPickable != null )
		{
			return false;
		}

		if ( pickable.CanPickup( Player ) )
		{
			if ( pickable is Item item )
			{
				HeldPickable = item;

				// Set the object's parent to the attachment object
				GameObject attachmentObject = SkinnedModelRenderer?.GetAttachmentObject( pickable.AttachmentBone ) ?? pickable.GameObject;
				HeldPickable.GameObject.SetParent( attachmentObject );
				HeldPickable.GameObject.LocalPosition = HeldPickable.AttachmentOffset;
				HeldPickable.GameObject.LocalRotation = HeldPickable.AttachmentRotation;

				// Disable the object's physics
				HeldPickable.GameObject.GetComponent<Rigidbody>( true ).Enabled = false;
				HeldPickable.GameObject.GetComponent<Collider>( true ).Enabled = false;

				// Set the player's hold type
				if ( CitizenAnimationHelper != null )
				{
					CitizenAnimationHelper.HoldType = HeldPickable.HoldType;
				}

				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Try to drop the held object
	/// </summary>
	/// <returns>True if the object was dropped, false otherwise</returns>
	public bool TryDrop()
	{
		if ( HeldPickable == null )
		{
			return false;
		}

		if ( HeldPickable.CanDrop( Player ) )
		{
			// Remove the object from the player's hand
			HeldPickable.GameObject.SetParent( null );

			// Re-enable the object's physics
			Rigidbody? rigidbody = HeldPickable.GameObject.GetComponent<Rigidbody>( true );
			if ( rigidbody != null )
			{
				rigidbody.Enabled = true;
			}

			Collider? collider = HeldPickable.GameObject.GetComponent<Collider>( true );
			if ( collider != null )
			{
				collider.Enabled = true;
			}

			if ( CitizenAnimationHelper != null )
			{
				CitizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			}

			HeldPickable = null;

			return true;
		}

		return false;
	}
}
