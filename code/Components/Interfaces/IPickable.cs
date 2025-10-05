using Sandbox.Citizen;

namespace Undercooked.Components.Interfaces;

/// <summary>
/// Interface for any object that can be picked up
/// </summary>
public interface IPickable
{
	GameObject GameObject { get; }

	// The bone to attach the pickable to
	string AttachmentBone { get; }

	// The hold type to use when picking up the pickable
	CitizenAnimationHelper.HoldTypes HoldType { get; }

	// The offset to use when picking up the pickable
	Vector3 AttachmentOffset { get; }

	// The rotation to use when picking up the pickable
	Rotation AttachmentRotation { get; }

	bool CanBePickedUp( Player by );

	void OnPickedUp( Player by );

	bool CanBeDepositedOn( IDepositable surface, Player by );

	void OnDeposited( IDepositable surface, Player by );

	void OnDropped();
}

