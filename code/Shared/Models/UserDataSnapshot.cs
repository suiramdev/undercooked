#nullable enable

namespace Undercooked;

public sealed class UserDataSnapshot
{
	public string UserId { get; set; } = string.Empty;

	public string DisplayName { get; set; } = string.Empty;

	public int Money { get; set; }

	public long Revision { get; set; }

	public float UpdatedAt { get; set; }
}
