#nullable enable

namespace Undercooked;

[Icon( "person" )]
public partial class Player : Component
{
	public static Player? Local { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		SetupCameraController();
	}

	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			// Assign this instance as the local player
			Local = this;
		}
	}

	protected override void OnDestroy()
	{
		if ( Local == this )
		{
			Local = null;
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		HandleInteractionInput();
		HandleMovementInput();
		HandleAnimation();
	}
}
