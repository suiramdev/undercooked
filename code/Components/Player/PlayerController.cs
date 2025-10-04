#nullable enable

using System;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Movement;

namespace Undercooked.Components;

[Icon( "directions_walk" )]
public class PlayerController : Component
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

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		Move();
		Animate();
	}

	private float GetMovementSpeed()
	{
		if ( !CharacterController.IsOnGround )
			return DefaultSpeed * AirSpeedMultiplier;

		return DefaultSpeed;
	}

	private void Move()
	{
		float movementSpeed = GetMovementSpeed();
		Vector3 inputDirection = Input.AnalogMove;

		Vector3 moveDirection = ApplyMovement( inputDirection, movementSpeed );
		ApplyGravity();
		HandleRotation( inputDirection, moveDirection );

		CharacterController.Move();
	}

	private Vector3 ApplyMovement( Vector3 inputDirection, float speed )
	{
		Vector3 moveDirection;

		if ( CameraController != null )
		{
			// Camera-relative movement: convert input to world space based on camera orientation
			Vector3 cameraForward = CameraController.CameraForward;
			Vector3 cameraRight = CameraController.CameraRight;
			moveDirection = (cameraForward * inputDirection.x + cameraRight * inputDirection.y).Normal;
		}
		else
		{
			// Fallback: use character's forward direction
			moveDirection = WorldRotation.Forward;
		}

		// Scale by input magnitude to support analog input (e.g., controller sticks)
		float inputMagnitude = Math.Min( inputDirection.Length, 1f );

		// Apply horizontal movement while preserving vertical velocity (for gravity/jumping)
		CharacterController.Velocity = new Vector3(
			moveDirection.x * speed * inputMagnitude,
			moveDirection.y * speed * inputMagnitude,
			CharacterController.Velocity.z );

		return moveDirection;
	}

	private void ApplyGravity()
	{
		if ( !CharacterController.IsOnGround )
			CharacterController.Velocity += Vector3.Down * Gravity * Time.Delta;
	}

	private void HandleRotation( Vector3 inputDirection, Vector3 moveDirection )
	{
		if ( inputDirection.Length <= MovementThreshold )
			return;

		// Smoothly rotate to face movement direction
		Rotation targetRotation = Rotation.LookAt( moveDirection, Vector3.Up );
		WorldRotation = Rotation.Lerp( WorldRotation, targetRotation, Time.Delta * RotationSpeed );
	}

	private void Animate()
	{
		CitizenAnimationHelper.WithVelocity( CharacterController.Velocity );
	}
}
