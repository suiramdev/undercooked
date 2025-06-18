#nullable enable

using Undercooked.Components.Enums;
using Undercooked.Components.Interfaces;

namespace Undercooked.Components;

public abstract class BaseUtensil : Item, IDepositable
{
	public abstract bool CanAccept( IPickable pickable, Player player );

	public abstract bool CanWithdraw( Player player );

	public abstract void OnDeposit( IPickable pickable, Player player );

	public abstract void OnWithdraw( IPickable pickable, Player player );

	public abstract IPickable? GetStoredPickable();
}
