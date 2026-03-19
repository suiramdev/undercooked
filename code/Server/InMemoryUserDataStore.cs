#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Undercooked;

public sealed class InMemoryUserDataStore : IUserDataStore
{
	private readonly object _gate = new();
	private readonly Dictionary<string, UserDataRecord> _records = new();

	public Task<UserDataRecord> GetOrCreateAsync(
		string userId,
		string displayName,
		CancellationToken cancellationToken = default )
	{
		lock ( _gate )
		{
			var normalizedUserId = NormalizeKey( userId );
			if ( _records.TryGetValue( normalizedUserId, out var existing ) )
			{
				if ( string.IsNullOrWhiteSpace( existing.DisplayName ) && !string.IsNullOrWhiteSpace( displayName ) )
				{
					existing.DisplayName = displayName;
				}

				return Task.FromResult( existing.Clone() );
			}

			var created = new UserDataRecord
			{
				UserId = userId,
				DisplayName = string.IsNullOrWhiteSpace( displayName ) ? userId : displayName,
				Money = BackendConVars.DefaultMoney,
				Revision = 1L,
				UpdatedAt = Time.Now
			};

			_records[normalizedUserId] = created.Clone();
			return Task.FromResult( created.Clone() );
		}
	}

	public Task<UserDataRecord> SaveAsync( UserDataRecord userData, CancellationToken cancellationToken = default )
	{
		lock ( _gate )
		{
			var copy = userData.Clone();
			copy.Revision = copy.Revision + 1L;
			copy.UpdatedAt = Time.Now;
			_records[NormalizeKey( copy.UserId )] = copy.Clone();
			return Task.FromResult( copy.Clone() );
		}
	}

	private static string NormalizeKey( string userId )
	{
		return (userId ?? string.Empty).Trim().ToLowerInvariant();
	}
}
