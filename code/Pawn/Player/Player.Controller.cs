#nullable enable

using System;
using Sandbox.Citizen;

namespace Undercooked;

public partial class Player
{
	[Property]
	[Header( "Movement Settings" )]
	public float DefaultSpeed { get; set; } = 100f;

	[Property]
	public float AirSpeedMultiplier { get; set; } = 0.5f;

	[Property]
	public float RotationSpeed { get; set; } = 10f;

	[Property]
	[Description( "Minimum threshold to prevent micro-movements from affecting rotation" )]
	public float MovementThreshold { get; set; } = 0.1f;

	[Property]
	[Description( "Gravity of the player" )]
	public float Gravity { get; set; } = 980f;

	[Property]
	[Header( "Dash Settings" )]
	[Description( "Input action used to trigger the dash" )]
	public string DashAction { get; set; } = "Run";

	[Property]
	[Description( "How long the dash burst lasts" )]
	public float DashDuration { get; set; } = 0.18f;

	[Property]
	[Description( "Minimum time between dash starts" )]
	public float DashCooldown { get; set; } = 0.75f;

	[Property]
	[Description( "Initial horizontal speed at the start of the dash" )]
	public float DashSpeed { get; set; } = 420f;

	[Property]
	[Description( "Horizontal speed kept at the end of the dash for a short slide" )]
	public float DashEndSpeed { get; set; } = 150f;

	[Property]
	[Range( 0f, 1f )]
	[Description( "How much movement input can bend the dash direction while active" )]
	public float DashSteering { get; set; } = 0.1f;

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required CharacterController CharacterController { get; set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public required CitizenAnimationHelper CitizenAnimationHelper { get; set; }

	[Property]
	[Group( "Components" )]
	[Description( "Optional camera controller for camera-relative movement" )]
	public PlayerCameraController? CameraController { get; set; }

	public bool IsDashing { get; private set; }

	private TimeSince _timeSinceDashStarted = 0f;
	private TimeSince _timeSinceLastDash = 999f;
	private Vector3 _dashDirection = Vector3.Zero;

	private void SetupCameraController()
	{
		if ( !IsProxy )
		{
			// Assign the first available PlayerCameraController from the scene to this player
			CameraController = Scene.Components.GetAll<PlayerCameraController>().FirstOrDefault();
		}
	}

	private float GetMovementSpeed()
	{
		if ( !CharacterController.IsOnGround )
			return DefaultSpeed * AirSpeedMultiplier;

		return DefaultSpeed;
	}

	private void HandleMovementInput()
	{
		if ( IsProxy )
			return;

		float movementSpeed = GetMovementSpeed();
		Vector3 inputDirection = Input.AnalogMove;

		TryStartDash( inputDirection );

		Vector3 moveDirection = ApplyMovement( inputDirection, movementSpeed );
		ApplyGravity();
		HandleRotation( inputDirection, moveDirection );

		CharacterController.Move();
	}

	private Vector3 ApplyMovement( Vector3 inputDirection, float speed )
	{
		Vector3 desiredMoveDirection = GetDesiredMoveDirection( inputDirection );
		Vector3 horizontalVelocity = desiredMoveDirection * speed;
		Vector3 moveDirection = desiredMoveDirection;

		if ( IsDashing )
		{
			(horizontalVelocity, moveDirection) = GetDashMovement( desiredMoveDirection, speed );
		}

		// Apply horizontal movement while preserving vertical velocity (for gravity/jumping)
		CharacterController.Velocity = new Vector3(
			horizontalVelocity.x,
			horizontalVelocity.y,
			CharacterController.Velocity.z );

		return moveDirection;
	}

	private Vector3 GetDesiredMoveDirection( Vector3 inputDirection )
	{
		if ( inputDirection.Length <= MovementThreshold )
			return Vector3.Zero;

		if ( CameraController != null )
		{
			// Camera-relative movement: convert input to world space based on camera orientation
			Vector3 cameraForward = CameraController.CameraForward;
			Vector3 cameraRight = CameraController.CameraRight;
			return (cameraForward * inputDirection.x + cameraRight * inputDirection.y).Normal;
		}

		return new Vector3( inputDirection.x, inputDirection.y, 0f ).Normal;
	}

	private void TryStartDash( Vector3 inputDirection )
	{
		if ( IsDashing )
			return;

		if ( !CharacterController.IsOnGround )
			return;

		if ( !Input.Pressed( DashAction ) )
			return;

		if ( _timeSinceLastDash < DashCooldown )
			return;

		Vector3 dashDirection = GetDashDirection( inputDirection );
		if ( dashDirection.Length <= MovementThreshold )
			return;

		_dashDirection = dashDirection;
		_timeSinceDashStarted = 0f;
		_timeSinceLastDash = 0f;
		IsDashing = true;
	}

	private Vector3 GetDashDirection( Vector3 inputDirection )
	{
		Vector3 desiredMoveDirection = GetDesiredMoveDirection( inputDirection );
		if ( desiredMoveDirection.Length > MovementThreshold )
			return desiredMoveDirection;

		Vector3 currentHorizontalVelocity = CharacterController.Velocity.WithZ( 0f );
		if ( currentHorizontalVelocity.Length > MovementThreshold )
			return currentHorizontalVelocity.Normal;

		Vector3 facingDirection = WorldRotation.Forward.WithZ( 0f );
		return facingDirection.Length > 0f
			? facingDirection.Normal
			: Vector3.Zero;
	}

	private (Vector3 HorizontalVelocity, Vector3 MoveDirection) GetDashMovement( Vector3 desiredMoveDirection, float speed )
	{
		float dashProgress = DashDuration <= 0f
			? 1f
			: Math.Clamp( _timeSinceDashStarted / DashDuration, 0f, 1f );

		if ( dashProgress >= 1f )
		{
			IsDashing = false;
			return (desiredMoveDirection * speed, desiredMoveDirection);
		}

		Vector3 dashDirection = _dashDirection;
		if ( DashSteering > 0f && desiredMoveDirection.Length > MovementThreshold )
		{
			dashDirection = Vector3.Lerp( _dashDirection, desiredMoveDirection, DashSteering ).Normal;
		}

		float dashSpeed = DashSpeed + (DashEndSpeed - DashSpeed) * dashProgress;
		return (dashDirection * dashSpeed, dashDirection);
	}

	private void ApplyGravity()
	{
		if ( !CharacterController.IsOnGround )
			CharacterController.Velocity += Vector3.Down * Gravity * Time.Delta;
	}

	private void HandleRotation( Vector3 inputDirection, Vector3 moveDirection )
	{
		if ( !IsDashing && inputDirection.Length <= MovementThreshold )
			return;

		if ( moveDirection.Length <= MovementThreshold )
			return;

		// Smoothly rotate to face movement direction
		Rotation targetRotation = Rotation.LookAt( moveDirection, Vector3.Up );
		WorldRotation = Rotation.Lerp( WorldRotation, targetRotation, Time.Delta * RotationSpeed );
	}

	private void HandleAnimation()
	{
		CitizenAnimationHelper.WithVelocity( CharacterController.Velocity );
	}
}
