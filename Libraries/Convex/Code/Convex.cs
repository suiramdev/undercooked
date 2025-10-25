
#nullable enable

using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Text;
using Sandbox;
using System.Net.Http.Json;

namespace Undercooked.Libraries;

/// <summary>
/// Response from a successful Convex function call
/// </summary>
public class ConvexSuccessResponse<T>
{
    [JsonPropertyName( "status" )]
    public string Status { get; set; } = "success";

    [JsonPropertyName( "value" )]
    public T? Value { get; set; }

    [JsonPropertyName( "logLines" )]
    public List<string>? LogLines { get; set; }
}

/// <summary>
/// Response from a failed Convex function call
/// </summary>
public class ConvexErrorResponse
{
    [JsonPropertyName( "status" )]
    public string Status { get; set; } = "error";

    [JsonPropertyName( "errorMessage" )]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName( "errorData" )]
    public JsonElement? ErrorData { get; set; }

    [JsonPropertyName( "logLines" )]
    public List<string>? LogLines { get; set; }
}

/// <summary>
/// Result of a Convex function call
/// </summary>
public class ConvexResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }
    public JsonElement? ErrorData { get; set; }
    public List<string>? LogLines { get; set; }
}

public partial class ConvexClient

{
    private readonly string _deploymentUrl;
    private string? _authToken;
    private WebSocket? _socket;

    public ConvexClient( string deploymentUrl )
    {
        _deploymentUrl = deploymentUrl.TrimEnd( '/' );
        KeepSocketAlive();
    }

    private void KeepSocketAlive()
    {
        if ( _socket is null || !_socket.IsConnected )
        {
            _socket?.Dispose();
            _socket = new WebSocket();
            var socketUrl = _deploymentUrl.TrimStart( "https://".ToCharArray() ).TrimStart( "http://".ToCharArray() );
            _socket.Connect( $"wss://{socketUrl}/api/sync" );
            _socket.OnDisconnected += OnSocketDisconnected;
            _socket.OnDataReceived += OnSocketDataReceived;
            _socket.OnMessageReceived += OnSocketMessageReceived;
        }
    }

    /// <summary>
    /// Sets the authentication token for API calls
    /// </summary>
    /// <param name="token">Bearer token from your auth provider</param>
    public void SetAuthToken( string token )
    {
        _authToken = token;
    }

    /// <summary>
    /// Clears the authentication token
    /// </summary>
    public void ClearAuthToken()
    {
        _authToken = null;
    }

    /// <summary>
    /// Executes a Convex query
    /// </summary>
    /// <param name="functionPath">Path to the function (e.g., "messages:list")</param>
    /// <param name="args">Named arguments to pass to the function</param>
    /// <typeparam name="T">Expected return type</typeparam>
    /// <returns>Result containing the value or error</returns>
    public async Task<ConvexResult<T>> QueryAsync<T>( string functionPath, object? args = null )
    {
        return await CallFunctionAsync<T>( "query", functionPath, args );
    }

    /// <summary>
    /// Executes a Convex mutation
    /// </summary>
    /// <param name="functionPath">Path to the function (e.g., "messages:send")</param>
    /// <param name="args">Named arguments to pass to the function</param>
    /// <typeparam name="T">Expected return type</typeparam>
    /// <returns>Result containing the value or error</returns>
    public async Task<ConvexResult<T>> MutationAsync<T>( string functionPath, object? args = null )
    {
        return await CallFunctionAsync<T>( "mutation", functionPath, args );
    }

    /// <summary>
    /// Executes a Convex action
    /// </summary>
    /// <param name="functionPath">Path to the function (e.g., "actions:sendEmail")</param>
    /// <param name="args">Named arguments to pass to the function</param>
    /// <typeparam name="T">Expected return type</typeparam>
    /// <returns>Result containing the value or error</returns>
    public async Task<ConvexResult<T>> ActionAsync<T>( string functionPath, object? args = null )
    {
        return await CallFunctionAsync<T>( "action", functionPath, args );
    }

    /// <summary>
    /// Executes any Convex function using the /api/run endpoint
    /// </summary>
    /// <param name="functionPath">Path to the function (e.g., "messages:list")</param>
    /// <param name="args">Named arguments to pass to the function</param>
    /// <typeparam name="T">Expected return type</typeparam>
    /// <returns>Result containing the value or error</returns>
    public async Task<ConvexResult<T>> RunAsync<T>( string functionPath, object? args = null )
    {
        var functionIdentifier = functionPath.Replace( ":", "/" );
        var url = $"{_deploymentUrl}/api/run/{functionIdentifier}";

        var requestBody = new
        {
            args = args ?? new { },
            format = "json"
        };

        return await ExecuteRequestAsync<T>( url, requestBody );
    }

    // TODO: Implement this
    // public async Task SubcribeQuerySetAsync( string functionPath, Action<JsonElement> onUpdate )
    // {
    //     var functionIdentifier = functionPath.Replace( ":", "/" );
    //     var json = JsonSerializer.Serialize( new
    //     {
    //         type = "ModifyQuerySet",
    //         path = functionIdentifier,
    //         args = args ?? new { },
    //         format = "json"
    //     } );

    //     _socket.Send( json );
    // }

    private async Task<ConvexResult<T>> CallFunctionAsync<T>( string functionType, string functionPath, object? args )
    {
        var url = $"{_deploymentUrl}/api/{functionType}";

        var requestBody = new
        {
            path = functionPath,
            args = args ?? new { },
            format = "json"
        };

        return await ExecuteRequestAsync<T>( url, requestBody );
    }

    private async Task<ConvexResult<T>> ExecuteRequestAsync<T>( string url, object requestBody )
    {
        try
        {
            // Ensure the WebSocket connection is active
            KeepSocketAlive();

            var json = JsonSerializer.Serialize( requestBody );

            // Build headers
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            // Add auth header if token is set
            if ( !string.IsNullOrEmpty( _authToken ) )
            {
                headers.Add( "Authorization", $"Bearer {_authToken}" );
            }

            // Make the request using s&box Http API
            var content = new StringContent( json, Encoding.UTF8, "application/json" );
            var response = await Http.RequestAsync( url,
                method: "POST",
                headers: headers,
                content: content
            );

            // Check if request was successful
            if ( response.StatusCode < HttpStatusCode.OK || response.StatusCode >= HttpStatusCode.MultipleChoices )
            {
                return new ConvexResult<T>
                {
                    IsSuccess = false,
                    ErrorMessage = $"HTTP request failed with status {response.StatusCode}: {response.Content}"
                };
            }

            var responseContent = response.Content;

            // Try to parse as error response first
            var doc = await responseContent.ReadFromJsonAsync<JsonDocument>();
            var root = doc?.RootElement;

            if ( root.HasValue && root.Value.TryGetProperty( "status", out var statusProp ) )
            {
                var status = statusProp.GetString();

                if ( status == "error" )
                {
                    var errorResponse = doc?.Deserialize<ConvexErrorResponse>();
                    return new ConvexResult<T>
                    {
                        IsSuccess = false,
                        ErrorMessage = errorResponse?.ErrorMessage,
                        ErrorData = errorResponse?.ErrorData,
                        LogLines = errorResponse?.LogLines
                    };
                }
                else if ( status == "success" )
                {
                    var successResponse = doc?.Deserialize<ConvexSuccessResponse<T>>();
                    return new ConvexResult<T>
                    {
                        IsSuccess = true,
                        Value = successResponse != null ? successResponse.Value : default,
                        LogLines = successResponse?.LogLines
                    };
                }
            }

            return new ConvexResult<T>
            {
                IsSuccess = false,
                ErrorMessage = "Invalid response format from Convex"
            };
        }
        catch ( Exception ex )
        {
            return new ConvexResult<T>
            {
                IsSuccess = false,
                ErrorMessage = $"Request failed: {ex.Message}"
            };
        }
    }

    private void OnSocketDisconnected( int status, string reason )
    {
        Log.Info( $"WebSocket disconnected: {status} {reason}" );
    }

    private void OnSocketDataReceived( Span<byte> data )
    {
        Log.Info( "WebSocket data received" );
    }

    private void OnSocketMessageReceived( string message )
    {
        Log.Info( "WebSocket message received: " + message );
    }
}