namespace Vizgql.Console;

public static class SchemaFromHttp
{
    public static async Task<string> ReadAsync(
        CommandLineOptions options,
        CancellationToken cancellationToken
    )
    {
        using var httpClient = new HttpClient();

        if (!string.IsNullOrEmpty(options.OAuthConfig))
        {
            options.HeaderName ??= "Authorization";
            options.HeaderToken = await GetOauthTokenAsync(options, cancellationToken);
        }

        if (!string.IsNullOrEmpty(options.HeaderName) && !string.IsNullOrEmpty(options.HeaderToken))
        {
            httpClient.DefaultRequestHeaders.Add(options.HeaderName, options.HeaderToken);
        }

        return await httpClient.GetStringAsync(options.Url, cancellationToken);
    }

    private static Task<string> GetOauthTokenAsync(
        CommandLineOptions options,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult("TODO");
    }
}
