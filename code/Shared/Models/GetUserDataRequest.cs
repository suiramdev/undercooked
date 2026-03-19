#nullable enable

namespace Undercooked;

public sealed class GetUserDataRequest
{
	public string UserId { get; set; } = string.Empty;

	public string DisplayName { get; set; } = string.Empty;
}
