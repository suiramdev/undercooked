using Undercooked.Components.Enums;

namespace Undercooked.Components.Interfaces;

/// <summary>
/// Interface for any object that can be used
/// </summary>
public interface IUsable
{
	GameObject GameObject { get; }

	UseType UseType { get; }

	bool CanUse( Player player );

	void OnUse( Player player );
}

