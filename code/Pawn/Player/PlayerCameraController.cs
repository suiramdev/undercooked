#nullable enable

using System;

namespace Undercooked.Components;

[Icon( "videocam" )]
[Title( "Player Camera Controller" )]
[Category( "Camera" )]
public sealed class PlayerCameraController : Component
{
    [Property]
    public required CameraComponent Camera { get; set; }

    [Property]
    [Category( "Positioning" )]
    [Description( "Camera distance from player when fully zoomed in" )]
    public float MinDistance { get; set; } = 300f;

    [Property]
    [Category( "Positioning" )]
    [Description( "Camera distance from player when fully zoomed out" )]
    public float MaxDistance { get; set; } = 600f;

    [Property]
    [Category( "Positioning" )]
    [Description( "Downward tilt angle in degrees when fully zoomed in (0 = level, 90 = straight down)" )]
    public float MinTiltAngle { get; set; } = 25f;

    [Property]
    [Category( "Positioning" )]
    [Description( "Downward tilt angle in degrees when fully zoomed out (0 = level, 90 = straight down)" )]
    public float MaxTiltAngle { get; set; } = 45f;

    [Property]
    [Category( "Positioning" )]
    [Description( "Height offset above player position" )]
    public float HeightOffset { get; set; } = 1.5f;

    [Property]
    [Category( "Positioning" )]
    [Description( "Initial yaw rotation around player (0 = behind player)" )]
    public float InitialYaw { get; set; } = 0f;

    [Property]
    [Category( "Follow Smoothing" )]
    [Description( "How quickly camera follows player position (higher = faster)" )]
    [Range( 0.1f, 20f )]
    public float FollowSpeed { get; set; } = 4f;

    [Property]
    [Category( "Zoom" )]
    [Description( "Minimum zoom distance" )]
    public float MinZoom { get; set; } = 0f;

    [Property]
    [Category( "Zoom" )]
    [Description( "Maximum zoom distance" )]
    public float MaxZoom { get; set; } = 1f;

    [Property]
    [Category( "Zoom" )]
    [Description( "Zoom speed multiplier" )]
    public float ZoomSpeed { get; set; } = 0.8f;

    [Property]
    [Category( "Zoom" )]
    [Description( "S-curve smoothing intensity (0 = linear, 1 = strong S-curve)" )]
    [Range( 0f, 1f )]
    public float ZoomSmoothness { get; set; } = 0.8f;

    [Property]
    [Category( "Rotation" )]
    [Description( "Rotation speed in degrees per second when holding key" )]
    public float ManualRotationSpeed { get; set; } = 90f;

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
    private Vector3 _targetPosition = Vector3.Zero;
    private Vector3 _smoothedPosition = Vector3.Zero;
    private float _currentYaw = 0f;
    private float _currentDistance;
    private float _targetDistance;

    /// <summary>
    /// Gets the current camera yaw rotation (useful for camera-relative player movement)
    /// </summary>
    public float CameraYaw => _currentYaw;

    /// <summary>
    /// Gets the camera's horizontal forward direction (ignoring tilt)
    /// </summary>
    public Vector3 CameraForward
    {
        get
        {
            float yawRad = _currentYaw * MathF.PI / 180f;
            return new Vector3( -MathF.Sin( yawRad ), MathF.Cos( yawRad ), 0f ).Normal;
        }
    }

    /// <summary>
    /// Gets the camera's horizontal right direction (ignoring tilt)
    /// </summary>
    public Vector3 CameraRight
    {
        get
        {
            float yawRad = _currentYaw * MathF.PI / 180f;
            return new Vector3( -MathF.Cos( yawRad ), -MathF.Sin( yawRad ), 0f ).Normal;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        _currentYaw = InitialYaw;
        _currentDistance = MaxZoom;
        _targetDistance = MaxZoom;

        if ( Player.Local != null )
        {
            _smoothedPosition = Player.Local.WorldPosition;
            _targetPosition = Player.Local.WorldPosition;
        }
        else
        {
            _smoothedPosition = WorldPosition;
            _targetPosition = WorldPosition;
        }

        UpdateCameraTransform();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if ( Player.Local == null )
            return;

        // Handle rotation input
        float rotationInput = 0f;
        if ( Input.Down( RotateCWAction ) )
            rotationInput += 1f;
        if ( Input.Down( RotateCCWAction ) )
            rotationInput -= 1f;

        Angles analogLook = Input.AnalogLook;
        rotationInput += analogLook.yaw;

        _currentYaw += rotationInput * ManualRotationSpeed * Time.Delta;
        _currentYaw = (_currentYaw % 360f + 360f) % 360f; // Normalize to 0-360

        // Handle zoom input
        float zoomInput = 0f;
        if ( Input.Down( ZoomInAction ) )
            zoomInput -= 1f;
        if ( Input.Down( ZoomOutAction ) )
            zoomInput += 1f;

        zoomInput += analogLook.pitch;

        if ( MathF.Abs( zoomInput ) > 0.01f )
        {
            _targetDistance += zoomInput * ZoomSpeed * Time.Delta;
            _targetDistance = Math.Clamp( _targetDistance, MinZoom, MaxZoom );
        }

        // Smooth zoom interpolation
        float smoothSpeed = 20f - (ZoomSmoothness * 15f);
        float zoomSmoothFactor = 1f - MathF.Exp( -smoothSpeed * Time.Delta );
        _currentDistance += (_targetDistance - _currentDistance) * zoomSmoothFactor;

        // Update target position
        _targetPosition = Player.Local.WorldPosition;

        // Smooth follow
        float followLerpFactor = 1f - MathF.Exp( -FollowSpeed * Time.Delta );
        _smoothedPosition = Vector3.Lerp( _smoothedPosition, _targetPosition, followLerpFactor );

        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        Vector3 pivotPosition = _smoothedPosition + Vector3.Up * HeightOffset;

        // Interpolate distance and tilt based on zoom (0 = zoomed in, 1 = zoomed out)
        float zoomT = (_currentDistance - MinZoom) / (MaxZoom - MinZoom);
        zoomT = Math.Clamp( zoomT, 0f, 1f );

        float actualDistance = MinDistance + (MaxDistance - MinDistance) * zoomT;
        float actualTiltAngle = MinTiltAngle + (MaxTiltAngle - MinTiltAngle) * zoomT;

        // Convert spherical coordinates to Cartesian offset
        float yawRad = _currentYaw * MathF.PI / 180f;
        float tiltRad = actualTiltAngle * MathF.PI / 180f;
        float horizontalDist = actualDistance * MathF.Cos( tiltRad );

        Vector3 offset = new(
            MathF.Sin( yawRad ) * horizontalDist,
            -MathF.Cos( yawRad ) * horizontalDist,
            actualDistance * MathF.Sin( tiltRad )
        );

        WorldPosition = pivotPosition + offset;
        WorldRotation = Rotation.LookAt( (pivotPosition - WorldPosition).Normal );
    }

    protected override void DrawGizmos()
    {
        base.DrawGizmos();

        if ( !Gizmo.IsSelected && !Gizmo.IsHovered )
            return;

        Vector3 pivotPosition = Player.Local != null
            ? Player.Local.WorldPosition + Vector3.Up * HeightOffset
            : WorldPosition + Vector3.Up * HeightOffset;

        // Draw pivot point (yellow)
        Gizmo.Transform = new Transform( Vector3.Zero );
        Gizmo.Draw.Color = Color.Yellow;
        Gizmo.Draw.LineSphere( pivotPosition, 5f );

        // Draw camera curve path (green)
        const int curveSteps = 30;
        Vector3? previousPoint = null;

        for ( int i = 0; i <= curveSteps; i++ )
        {
            float t = i / (float)curveSteps;
            float zoomDistance = MinZoom + (MaxZoom - MinZoom) * t;
            float zoomT = Math.Clamp( (zoomDistance - MinZoom) / (MaxZoom - MinZoom), 0f, 1f );

            float actualDistance = MinDistance + (MaxDistance - MinDistance) * zoomT;
            float actualTiltAngle = MinTiltAngle + (MaxTiltAngle - MinTiltAngle) * zoomT;

            float yawRad = _currentYaw * MathF.PI / 180f;
            float tiltRad = actualTiltAngle * MathF.PI / 180f;
            float horizontalDist = actualDistance * MathF.Cos( tiltRad );

            Vector3 offset = new(
                MathF.Sin( yawRad ) * horizontalDist,
                -MathF.Cos( yawRad ) * horizontalDist,
                actualDistance * MathF.Sin( tiltRad )
            );

            Vector3 curvePoint = pivotPosition + offset;

            if ( previousPoint.HasValue )
            {
                Gizmo.Draw.Color = Color.Green;
                Gizmo.Draw.Line( previousPoint.Value, curvePoint );
            }

            previousPoint = curvePoint;
        }
    }
}
