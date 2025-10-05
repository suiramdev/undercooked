#nullable enable

using Sandbox.Citizen;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public class PlayerSlot : Component, IDepositable
{
	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required SkinnedModelRenderer SkinnedModelRenderer { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required CitizenAnimationHelper CitizenAnimationHelper { get; set; }

	[Property]
	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	private IPickable? StoredPickable { get; set; }

	public bool Empty => StoredPickable is null;

	[Rpc.Host]
	public void Deposit( IPickable pickable, Player player )
	{
		if ( !Empty ) return;

		StoredPickable = pickable;

		// Attach item to the appropriate hand bone
		var ent = pickable.GameObject;
		GameObject attachmentObject = SkinnedModelRenderer.GetAttachmentObject( pickable.AttachmentBone );
		ent.SetParent( attachmentObject );
		ent.LocalPosition = pickable.AttachmentOffset;
		ent.LocalRotation = pickable.AttachmentRotation;

		// Disable the object's physics
		var rigidbody = ent.GetComponent<Rigidbody>( true );
		if ( rigidbody != null )
			rigidbody.Enabled = false;

		var collider = ent.GetComponent<Collider>( true );
		if ( collider != null )
			collider.Enabled = false;

		// Set the player's hold type
		CitizenAnimationHelper.HoldType = pickable.HoldType;

		pickable.OnPickedUp( player );

		return;
	}

	// Look at what we're holding without removing it
	public IPickable? GetPickable() => StoredPickable;

	// Remove and return the held item
	public IPickable? TakePickable()
	{
		var temp = StoredPickable;
		StoredPickable = default;
		CitizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		return temp;
	}

	[Rpc.Host]
	public void DropPickable()
	{
		var pickable = TakePickable();
		if ( pickable is null ) return;

		var ent = pickable.GameObject;

		ent.SetParent( null );
		ent.WorldPosition = GameObject.WorldPosition + GameObject.WorldRotation.Forward * 30.0f;
		ent.WorldRotation = GameObject.WorldRotation;

		// Re-enable the object's physics
		var rigidbody = ent.GetComponent<Rigidbody>( true );
		if ( rigidbody != null )
			rigidbody.Enabled = true;

		var collider = ent.GetComponent<Collider>( true );
		if ( collider != null )
			collider.Enabled = true;

		pickable.OnDropped();
	}
}
