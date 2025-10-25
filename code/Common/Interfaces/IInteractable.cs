
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

	string? GetInteractionText( Player by );

	void TryInteract( Player by );

	string? GetAlternateInteractionText( Player by );

	void TryAlternateInteract( Player by );
}
