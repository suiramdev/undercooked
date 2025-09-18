#nullable enable

namespace Undercooked.Components.Interfaces;

/// <summary>
/// Interface for any object that can be used to deposit a pickable
/// </summary>
public interface ITransferable : IDepositable
{
	bool TryTransfer( IDepositable depositable, Player by );
}
