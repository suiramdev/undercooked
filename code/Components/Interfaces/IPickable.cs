using Sandbox.Citizen;

namespace Undercooked.Components.Interfaces;

/// <summary>
/// Interface for any object that can be picked up
/// </summary>
public interface IPickable
{
	GameObject GameObject { get; }

	string AttachmentBone { get; }

	CitizenAnimationHelper.HoldTypes HoldType { get; }

	Vector3 AttachmentOffset { get; }

	Rotation AttachmentRotation { get; }

	void OnDeposit( IDepositable depositable );

	void OnDrop();
}

