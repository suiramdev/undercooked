
#nullable enable

using Undercooked.Components.Enums;

namespace Undercooked.Components.Interfaces;

/// <summary>
/// Interface for any object that can be interacted with
/// </summary>
public interface IInteractable
{
	GameObject GameObject { get; }

	InteractionType InteractionType { get; }

	InteractionType AlternateInteractionType { get; }

	void TryInteract( Player by );

	void TryAlternateInteract( Player by );
}
