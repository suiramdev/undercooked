#nullable enable

using System;

namespace Undercooked.Components;

[Icon( "videocam" )]
public sealed class InteriorCameraController : Component
{
    [Property]
    public required CameraComponent Camera { get; set; }

    [Property]
    [Category( "Pivot & Angles" )]
    public GameObject? RoomCenter { get; set; }

    [Property]
    [Category( "Pivot & Angles" )]
    [Description( "Downward tilt of the camera in degrees" )]
    public float TiltDegrees { get; set; } = 20f;

    [Property]
    [Category( "Pivot & Angles" )]
    [Description( "Rotation speed in degrees per second when holding key" )]
    public float RotationSpeed { get; set; } = 90f;

    [Property]
    [Category( "Zoom" )]
    [Description( "Zoom speed in units per second when holding key" )]
    public float ZoomSpeed { get; set; } = 3f;

    [Property]
    [Category( "Zoom" )]
    [Description( "Minimum zoom distance (closest to pivot)" )]
    public float MinZoom { get; set; } = 4.8f;

    [Property]
    [Category( "Zoom" )]
    [Description( "Maximum zoom distance (farthest from pivot)" )]
    public float MaxZoom { get; set; } = 7.5f;

    [Property]
    [Category( "Zoom" )]
    [Description( "Starting zoom distance" )]
    public float StartZoom { get; set; } = 6f;

    [Property]
    [Category( "Player Bias" )]
    [Description( "Optional player reference to nudge camera toward" )]
    public GameObject? Player { get; set; }

    [Property]
    [Category( "Player Bias" )]
    [Range( 0f, 1f )]
    [Description( "0 = room center, 1 = follow player completely" )]
    public float FollowBias { get; set; } = 0.35f;

    [Property]
    [Category( "Player Bias" )]
    [Description( "Speed of bias smoothing" )]
    public float FollowLerp { get; set; } = 7f;

    [Property]
    [Category( "Input" )]
    [Description( "Action to rotate camera clockwise" )]
    public string RotateCWAction { get; set; } = "RotateCW";

    [Property]
    [Category( "Input" )]
    [Description( "Action to rotate camera counter-clockwise" )]
    public string RotateCCWAction { get; set; } = "RotateCCW";

    [Property]
    [Category( "Input" )]
    [Description( "Action to zoom in (closer)" )]
    public string ZoomInAction { get; set; } = "ZoomIn";

    [Property]
    [Category( "Input" )]
    [Description( "Action to zoom out (farther)" )]
    public string ZoomOutAction { get; set; } = "ZoomOut";

    // Internal state
    private int _quadrant = 0; // 0=Front, 1=Right, 2=Back, 3=Left (clockwise rotation)
    private Vector3 _pivotPosition = Vector3.Zero;
    private float _currentYaw = 0f; // Current rotation angle in degrees
    private float _currentZoomDistance;

    protected override void OnAwake()
    {
        base.OnAwake();

        // Get camera component
        Camera = Components.Get<CameraComponent>();

        // Default room center to this GameObject's starting position
        if ( RoomCenter == null )
        {
            RoomCenter = GameObject;
        }

        // Initialize zoom
        _currentZoomDistance = Math.Clamp( StartZoom, MinZoom, MaxZoom );

        // Initialize pivot position
        if ( RoomCenter != null )
        {
            if ( Player != null && FollowBias > 0f )
            {
                _pivotPosition = Vector3.Lerp( RoomCenter.WorldPosition, Player.WorldPosition, Math.Clamp( FollowBias, 0f, 1f ) );
            }
            else
            {
                _pivotPosition = RoomCenter.WorldPosition;
            }
        }

        // Set initial rotation (Front quadrant)
        _currentYaw = 0f;
        UpdateRotation();

        // Position camera using forward vector
        UpdateCameraPosition();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // Update pivot position (follow player bias)
        if ( RoomCenter != null )
        {
            if ( Player != null && FollowBias > 0f )
            {
                Vector3 target = Vector3.Lerp( RoomCenter.WorldPosition, Player.WorldPosition, Math.Clamp( FollowBias, 0f, 1f ) );
                _pivotPosition = Vector3.Lerp( _pivotPosition, target, 1f - MathF.Exp( -FollowLerp * Time.Delta ) );
            }
            else
            {
                _pivotPosition = RoomCenter.WorldPosition;
            }
        }

        // Handle continuous rotation input
        float rotationInput = 0f;

        // Keyboard input (digital)
        if ( Input.Down( RotateCWAction ) )
        {
            rotationInput += 1f;
        }
        if ( Input.Down( RotateCCWAction ) )
        {
            rotationInput -= 1f;
        }

        // Gamepad input (analog - right stick horizontal)
        Angles analogLook = Input.AnalogLook;
        rotationInput += analogLook.yaw; // Right stick horizontal axis

        // Apply rotation with adaptive speed
        _currentYaw += rotationInput * RotationSpeed * Time.Delta;

        // Normalize yaw to 0-360 range
        _currentYaw = (_currentYaw % 360f + 360f) % 360f;

        // Update quadrant based on current rotation
        _quadrant = (int)(_currentYaw / 90f) % 4;

        // Apply rotation
        UpdateRotation();

        // Handle continuous zoom input
        float zoomInput = 0f;

        // Keyboard input (digital)
        if ( Input.Down( ZoomInAction ) )
        {
            zoomInput -= 1f; // Zoom in = decrease distance
        }
        if ( Input.Down( ZoomOutAction ) )
        {
            zoomInput += 1f; // Zoom out = increase distance
        }

        // Gamepad input (analog - right stick vertical)
        zoomInput -= analogLook.pitch; // Push up = zoom in, push down = zoom out

        // Apply zoom with adaptive speed
        _currentZoomDistance += zoomInput * ZoomSpeed * Time.Delta;

        // Clamp zoom distance
        _currentZoomDistance = Math.Clamp( _currentZoomDistance, MinZoom, MaxZoom );

        // Update camera position based on forward vector
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // Position camera behind pivot using forward vector
        // Camera looks toward pivot from distance away
        WorldPosition = _pivotPosition - WorldRotation.Forward * _currentZoomDistance;
    }

    private void UpdateRotation()
    {
        WorldRotation = Rotation.From( TiltDegrees, _currentYaw, 0f );
    }
}