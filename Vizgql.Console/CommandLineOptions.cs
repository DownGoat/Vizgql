using CommandLine;

namespace Vizgql.Console;

public sealed class CommandLineOptions
{
    [Option('f', "file", Required = false, HelpText = "Path to the file to be parsed.")]
    public string? InputPath { get; set; }

    [Option(
        'u',
        "url",
        Required = false,
        HelpText = "URL from which text will be downloaded for parsing."
    )]
    public string? Url { get; set; }

    [Option(
        'n',
        "header-name",
        Required = false,
        HelpText = "HTTP header name for authentication.",
        Default = "Authorization"
    )]
    public string? HeaderName { get; set; }

    [Option(
        't',
        "header-token",
        Required = false,
        HelpText = "HTTP header token for authentication."
    )]
    public string? HeaderToken { get; set; }

    [Option('o', "oauth", Required = false, HelpText = "OAuth2 configuration for token retrieval.")]
    public string? OAuthConfig { get; set; }

    [Option(
        'p',
        "output",
        Required = false,
        HelpText = "Path to output file where results will be written."
    )]
    public string? OutputPath { get; set; }
}
