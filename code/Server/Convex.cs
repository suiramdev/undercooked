#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Undercooked.Libraries.Convex;

namespace Undercooked.Server;

public static partial class Convex
{
    public static readonly ConvexClient Client = new( "http://localhost:8080" );

    public static async Task<T> WithAuthToken<T>( Func<Task<T>> action )
    {
        var result = await Client.ActionAsync<JsonElement>( "auth:signIn", new { steamId = Rpc.Caller.SteamId.Value.ToString(), token = await Sandbox.Services.Auth.GetToken( "Convex" ) } );
        var token = result.Value.GetProperty( "token" );
        var tokenString = token.GetString();
        if ( tokenString is not null )
        {
            Client.SetAuthToken( tokenString );
        }

        return await action();
    }
}