namespace Undercooked;

/// <summary>
/// Represents the type of interaction of an object
/// </summary>
public enum InteractionType
{
	Press,
	Hold,
	Release,
	/// <summary>
	/// One button press to start; keeps performing alternate interact each frame until the player moves or loses focus.
	/// </summary>
	PressToStartCancelOnMove,
}
