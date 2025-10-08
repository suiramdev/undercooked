#nullable enable

using System;
using System.Text.Json;

namespace Undercooked.Server;

public static partial class Convex
{
    [Rpc.Host]
    public static async void GetMyProfile()
    {
        try
        {
            var result = await WithAuthToken( () => Client.QueryAsync<JsonElement>( "users:getMyProfile" ) );
            if ( result.IsSuccess )
            {
                Log.Info( result.Value );
            }
            else
            {
                Log.Error( result.ErrorMessage );
            }
        }
        catch ( Exception ex )
        {
            Log.Error( ex.Message );
        }
    }
}