using System.Text.Json;
using Microsoft.Identity.Client;

namespace Vizgql.Console;

public static class SchemaFromHttp
{
    public static async Task<string> ReadAsync(
        CommandLineOptions options,
        CancellationToken cancellationToken
    )
    {
        using var httpClient = new HttpClient();

        // if (!string.IsNullOrEmpty(options.OAuthConfig))
        // {
        //     options.HeaderName ??= "Authorization";
        //     options.HeaderToken = await GetOauthTokenAsync(options, cancellationToken);
        // }

        if (!string.IsNullOrEmpty(options.HeaderName) && !string.IsNullOrEmpty(options.HeaderToken))
        {
            httpClient.DefaultRequestHeaders.Add(options.HeaderName, options.HeaderToken);
        }

        return await httpClient.GetStringAsync(options.Url, cancellationToken);
    }

    // private static async Task<string> GetOauthTokenAsync(
    //     CommandLineOptions options,
    //     CancellationToken cancellationToken
    // )
    // {
    //     return await FetchTokenAsync(options.OAuthConfig!, cancellationToken);
    // }

    private static async Task<string> FetchTokenAsync(
        string optionsOAuthConfig,
        CancellationToken cancellationToken
    )
    {
        var jsonContent = await File.ReadAllTextAsync(optionsOAuthConfig, cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var oauthConfig = JsonSerializer.Deserialize<OAuthConfig>(jsonContent, options);

        var app = PublicClientApplicationBuilder
            .Create(oauthConfig.ClientId)
            .WithAuthority(new Uri(oauthConfig.TokenUrl))
            .WithDefaultRedirectUri() // This uses "https://login.microsoftonline.com/common/oauth2/nativeclient" as the redirect URI.
            .Build();

        AuthenticationResult result;

        try
        {
            var accounts = await app.GetAccountsAsync();
            result = await app.AcquireTokenSilent(oauthConfig.Scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // UI interaction is required. This will show a dialog for username/password and any MFA challenges.
            result = await app.AcquireTokenInteractive(oauthConfig.Scopes).ExecuteAsync();
        }

        return result.AccessToken;
    }

    private static async Task<OAuthConfig?> LoadOAuthConfigAsync(
        string jsonConfigPath,
        CancellationToken cancellationToken
    )
    {
        var jsonContent = await File.ReadAllTextAsync(jsonConfigPath, cancellationToken);
        var oauthConfig = JsonSerializer.Deserialize<OAuthConfig>(
            jsonContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (oauthConfig is null)
        {
            throw new Exception("Not able to read oauth config");
        }

        return oauthConfig;
    }
}

public record OAuthConfig(string TokenUrl, string ClientId, string[] Scopes);
