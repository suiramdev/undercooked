#nullable enable

using System.Linq;
using System.Threading.Tasks;

namespace Undercooked;

[Icon( "storage" )]
[Title( "Backend Runtime" )]
public sealed class BackendRuntime : Component
{
	public static BackendRuntime? Instance { get; private set; }

	[Property]
	[Group( "Components" )]
	[RequireComponent]
	public BackendGateway Gateway { get; set; } = null!;

	private IUserDataStore? _userDataStore;
	private IUserDataService? _userDataService;
	private BackendProviderMode? _resolvedMode;

	public BackendProviderMode ProviderMode => BackendProviderModeParser.Parse( BackendConVars.ProviderMode );

	public IUserDataService UserDataService
	{
		get
		{
			var mode = ProviderMode;
			if ( _userDataService is null || _resolvedMode != mode )
			{
				_resolvedMode = mode;
				_userDataService = CreateUserDataService( mode );
			}

			return _userDataService;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Instance = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( Networking.IsHost && !GameObject.Network.Active )
		{
			GameObject.NetworkSpawn();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Instance == this )
		{
			Instance = null;
		}
	}

	public static BackendRuntime EnsureAvailable( Scene scene )
	{
		var existing = scene.GetAllComponents<BackendRuntime>().FirstOrDefault();
		if ( existing is not null )
			return existing;

		var runtimeObject = scene.CreateObject();
		runtimeObject.Name = "Backend Runtime";
		return runtimeObject.AddComponent<BackendRuntime>();
	}

	public void QueueOrderReward( Player player, int amount )
	{
		_ = RewardPlayerAsync( player, amount, "OrderCompletion" );
	}

	public async Task<UserDataSnapshot> RewardPlayerAsync( Player player, int amount, string reason )
	{
		var snapshot = await UserDataService.AddMoneyAsync(
			BackendUserIdentity.ResolveUserId( player ),
			amount,
			reason,
			BackendUserIdentity.ResolveDisplayName( player ) );

		player.ApplyBackendUserData( snapshot );
		return snapshot;
	}

	private IUserDataService CreateUserDataService( BackendProviderMode mode )
	{
		return mode switch
		{
			BackendProviderMode.RestApi => new RestApiProvider(),
			_ => new LocalServerProvider( _userDataStore ??= new InMemoryUserDataStore() )
		};
	}
}
