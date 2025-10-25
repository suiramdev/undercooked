#nullable enable

using Sandbox.Citizen;

namespace Undercooked;

public partial class Player : IDepositable, ITransferable
{
	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required SkinnedModelRenderer SkinnedModelRenderer { get; set; }

	[Change( nameof( OnStoredPickableChanged ) )]
	[Sync( SyncFlags.FromHost )]
	public IPickable? StoredPickable { get; private set; }

	public bool CanAccept( IPickable _ )
	{
		return StoredPickable is null;
	}

	[Rpc.Host]
	public void TryDeposit( IPickable pickable )
	{
		if ( !CanAccept( pickable ) ) return;

		StoredPickable = pickable;

		// Attach item to the appropriate hand bone
		GameObject attachmentObject = SkinnedModelRenderer.GetAttachmentObject( pickable.AttachmentBone );
		pickable.GameObject.SetParent( attachmentObject );
		pickable.GameObject.LocalPosition = pickable.AttachmentOffset;
		pickable.GameObject.LocalRotation = pickable.AttachmentRotation;

		// Disable the object's physics
		var rigidbody = pickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody != null )
			rigidbody.Enabled = false;

		var collider = pickable.GameObject.GetComponent<Collider>( true );
		if ( collider != null )
			collider.Enabled = false;

		pickable.OnDeposit( this );
	}

	[Rpc.Host]
	public void TryTransfer( IDepositable target )
	{
		if ( StoredPickable is null ) return;
		if ( !target.CanAccept( StoredPickable ) ) return;

		var pickable = StoredPickable;
		StoredPickable = null;

		target.TryDeposit( pickable );
	}

	[Rpc.Host]
	public void Drop()
	{
		if ( StoredPickable is null ) return;

		var pickable = StoredPickable;
		StoredPickable = null;

		pickable.GameObject.SetParent( null );
		pickable.GameObject.WorldPosition = GameObject.WorldPosition + GameObject.WorldRotation.Forward * 30.0f;
		pickable.GameObject.WorldRotation = GameObject.WorldRotation;

		// Re-enable the object's physics
		var rigidbody = pickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody != null )
			rigidbody.Enabled = true;

		var collider = pickable.GameObject.GetComponent<Collider>( true );
		if ( collider != null )
			collider.Enabled = true;

		pickable.OnDrop();
	}

	protected void OnStoredPickableChanged( IPickable? _, IPickable? newPickable )
	{
		CitizenAnimationHelper.HoldType = newPickable?.HoldType ?? CitizenAnimationHelper.HoldTypes.None;
	}
}
