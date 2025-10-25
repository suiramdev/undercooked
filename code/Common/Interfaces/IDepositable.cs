#nullable enable

namespace Undercooked;

/// <summary>
/// Interface for any object that can be used to deposit a pickable
/// </summary>
public interface IDepositable
{
	GameObject GameObject { get; }

	bool CanAccept( IPickable pickable );

	void TryDeposit( IPickable pickable );
}
