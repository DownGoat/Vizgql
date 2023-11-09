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

    [Option(
        'p',
        "policies",
        Required = false,
        HelpText = "comma-separated list of policies to apply to the schema."
    )]
    public string? Policies { get; set; }

    [Option(
        'r',
        "roles",
        Required = false,
        HelpText = "comma-separated list of roles to apply to the schema."
    )]
    public string? Roles { get; set; }

    [Option(
        "validations",
        Required = false,
        HelpText = "print out any validation errors",
        Default = false
    )]
    public bool Validations { get; set; }

    [Option(
        "unique-constraints",
        Required = false,
        HelpText = "Prints all the unique constrains as a comma separated list.",
        Default = false
    )]
    public bool UniqueConstraints { get; set; }

    public string[]? GetRoles()
    {
        return Roles?.Split(',');
    }

    public string[]? GetPolicies()
    {
        return Policies?.Split(',');
    }
}
