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
	public float RunSpeedMultiplier { get; set; } = 2f;

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

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		Move();
		Animate();
	}

	private float GetMovementSpeed()
	{
		if ( !CharacterController.IsOnGround )
		{
			return DefaultSpeed * AirSpeedMultiplier;
		}

		return Input.Down( "Run" ) ? DefaultSpeed * RunSpeedMultiplier : DefaultSpeed;
	}

	private void Move()
	{
		float movementSpeed = GetMovementSpeed();
		Vector3 inputDirection = Input.AnalogMove;

		ApplyMovement( inputDirection, movementSpeed );
		ApplyGravity();
		HandleRotation( inputDirection );

		CharacterController.Move();
	}

	private void ApplyMovement( Vector3 inputDirection, float speed )
	{
		// Convert input direction to world space movement direction
		// We use the forward vector and scale it by input magnitude
		Vector3 moveDirection = WorldRotation.Forward * inputDirection.Length;

		// Apply movement speed to horizontal velocity while preserving vertical velocity
		// This allows gravity and jumping to work independently of movement
		CharacterController.Velocity = new Vector3(
			moveDirection.x * speed,
			moveDirection.y * speed,
			CharacterController.Velocity.z );
	}

	private void ApplyGravity()
	{
		if ( !CharacterController.IsOnGround )
		{
			CharacterController.Velocity += Vector3.Down * Gravity * Time.Delta;
		}
	}

	private void HandleRotation( Vector3 inputDirection )
	{
		if ( inputDirection.Length <= MovementThreshold )
		{
			return;
		}

		// Convert 2D input vector to angle in degrees (arctangent of y/x)
		float targetAngle = MathF.Atan2( inputDirection.y, inputDirection.x ) * 180 / MathF.PI;

		// Create rotation around vertical axis based on calculated angle
		Rotation targetRotation = Rotation.FromYaw( targetAngle );

		// Smooth interpolation between current and target rotation
		// Time.Delta * RotationSpeed controls rotation rate independent of framerate
		WorldRotation = Rotation.Lerp( WorldRotation, targetRotation, Time.Delta * RotationSpeed );
	}

	private void Animate()
	{
		CitizenAnimationHelper.WithVelocity( CharacterController.Velocity );
	}
}
