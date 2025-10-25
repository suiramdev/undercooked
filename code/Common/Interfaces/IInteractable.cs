
#nullable enable

namespace Undercooked;

/// <summary>
/// Interface for any object that can be interacted with
/// </summary>
public interface IInteractable
{
	GameObject GameObject { get; }

	InteractionType InteractionType { get; }

	InteractionType AlternateInteractionType { get; }

	/// <summary>
	/// The text to display for the primary interaction (e.g., "Pickup", "Chop", "Use")
	/// </summary>
	string? InteractionText { get; }

	/// <summary>
	/// The text to display for the alternate interaction (e.g., "Drop", "Throw")
	/// </summary>
	string? AlternateInteractionText { get; }

	bool CanInteract( Player by );

	void TryInteract( Player by );

	bool CanAlternateInteract( Player by );

	void TryAlternateInteract( Player by );
}
