namespace MldevDashboard.Api.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Issuer { get; set; } = "MldevDashboard";

    public string Audience { get; set; } = "MldevDashboard.Web";

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
