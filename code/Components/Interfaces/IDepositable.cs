#nullable enable

namespace Undercooked.Components.Interfaces;

/// <summary>
/// Interface for any object that can be used to deposit a pickable
/// </summary>
public interface IDepositable
{
	bool CanAccept( IPickable pickable, Player player );

	bool CanWithdraw( Player player );

	void OnDeposit( IPickable pickable, Player player );

	void OnWithdraw( IPickable pickable, Player player );

	IPickable? GetStoredPickable();
}

