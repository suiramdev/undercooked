#nullable enable

namespace Undercooked.Components.Interfaces;

/// <summary>
/// Interface for any object that can be used to deposit a pickable
/// </summary>
public interface IDepositable
{
	GameObject GameObject { get; }

	bool Empty { get; }

	void Deposit( IPickable pickable, Player by );

	IPickable? GetPickable();

	IPickable? TakePickable();
}
