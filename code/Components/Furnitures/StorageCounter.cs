#nullable enable

using Sandbox;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

[Icon( "inventory" )]
public class StorageCounter : Component, IDepositable
{
	[Property]
	[Description( "This is a reference point for positioning the displayed item" )]
	public GameObject? ItemAnchorPoint { get; set; }

	[Property]
	[ReadOnly]
	public IPickable? StoredPickable { get; set; }

	public bool CanAccept( IPickable pickable, Player player )
	{
		return StoredPickable == null || (StoredPickable is IDepositable depositable && depositable.CanAccept( pickable, player ));
	}

	public bool CanWithdraw( Player player )
	{
		return StoredPickable != null;
	}

	public void OnDeposit( IPickable pickable, Player player )
	{
		if ( StoredPickable != null && StoredPickable is IDepositable depositable )
		{
			depositable.OnDeposit( pickable, player );
			return;
		}

		StoredPickable = pickable;

		StoredPickable.GameObject.SetParent( ItemAnchorPoint ?? GameObject );
		StoredPickable.GameObject.LocalPosition = Vector3.Zero;
		StoredPickable.GameObject.LocalRotation = Rotation.Identity;

		Rigidbody? rigidbody = StoredPickable.GameObject.GetComponent<Rigidbody>( true );
		if ( rigidbody != null )
		{
			rigidbody.Enabled = false;
		}

		Collider? collider = StoredPickable.GameObject.GetComponent<Collider>( true );
		if ( collider != null )
		{
			collider.Enabled = false;
		}
	}

	public void OnWithdraw( IPickable pickable, Player player )
	{
		StoredPickable = null;
	}

	public IPickable? GetStoredPickable()
	{
		return StoredPickable;
	}
}
