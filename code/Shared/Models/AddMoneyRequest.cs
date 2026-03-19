#nullable enable

namespace Undercooked;

public sealed class AddMoneyRequest
{
	public string UserId { get; set; } = string.Empty;

	public int Amount { get; set; }

	public string Reason { get; set; } = string.Empty;

	public string DisplayName { get; set; } = string.Empty;
}
