#nullable enable

namespace Undercooked;

/// <summary>
/// Interface for any object that can be used to deposit a pickable
/// </summary>
public interface ITransferable : IDepositable
{
	void TryTransfer( IDepositable depositable );
}
