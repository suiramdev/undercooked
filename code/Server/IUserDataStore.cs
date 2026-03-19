#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace Undercooked;

public interface IUserDataStore
{
	Task<UserDataRecord> GetOrCreateAsync(
		string userId,
		string displayName,
		CancellationToken cancellationToken = default );

	Task<UserDataRecord> SaveAsync( UserDataRecord userData, CancellationToken cancellationToken = default );
}
