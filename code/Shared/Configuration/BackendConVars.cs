#nullable enable

namespace Undercooked;

public static class BackendConVars
{
	[ConVar( "undercooked_backend_mode" )]
	public static string ProviderMode { get; set; } = "local";

	[ConVar( "undercooked_backend_rest_base_url" )]
	public static string RestApiBaseUrl { get; set; } = "http://localhost:8080/api/users";

	[ConVar( "undercooked_backend_rest_api_key" )]
	public static string RestApiKey { get; set; } = string.Empty;

	[ConVar( "undercooked_backend_default_money" )]
	public static int DefaultMoney { get; set; } = 0;

	[ConVar( "undercooked_backend_order_reward" )]
	public static int OrderReward { get; set; } = 100;
}
