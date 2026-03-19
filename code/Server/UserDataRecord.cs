#nullable enable

namespace Undercooked;

public sealed class UserDataRecord
{
	public string UserId { get; set; } = string.Empty;

	public string DisplayName { get; set; } = string.Empty;

	public int Money { get; set; }

	public long Revision { get; set; } = 1L;

	public float UpdatedAt { get; set; }

	public UserDataRecord Clone()
	{
		return new UserDataRecord
		{
			UserId = UserId,
			DisplayName = DisplayName,
			Money = Money,
			Revision = Revision,
			UpdatedAt = UpdatedAt
		};
	}

	public UserDataSnapshot ToSnapshot()
	{
		return new UserDataSnapshot
		{
			UserId = UserId,
			DisplayName = DisplayName,
			Money = Money,
			Revision = Revision,
			UpdatedAt = UpdatedAt
		};
	}
}
