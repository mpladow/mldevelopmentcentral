namespace MldevDashboard.Api.Common;

public static class ApplicationRoleNames
{
    public const string GlobalAdmin = "GlobalAdmin";

    public static string CreateSystemRoleName(string systemLabel, string suffix)
    {
        var nameCharacters = systemLabel
            .Trim()
            .Where(char.IsLetterOrDigit)
            .ToArray();

        var prefix = new string(nameCharacters);
        return string.IsNullOrWhiteSpace(prefix)
            ? $"System{suffix}"
            : $"{prefix}{suffix}";
    }
}
