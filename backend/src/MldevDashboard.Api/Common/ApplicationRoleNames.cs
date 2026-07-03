namespace MldevDashboard.Api.Common;

public static class ApplicationRoleNames
{
    public const string GlobalAdmin = "GlobalAdmin";
    public const string GlobalUser = "GlobalUser";
    public const string GlobalViewer = "GlobalViewer";
    public const string WarmasterAdmin = "WarmasterAdmin";
    public const string WarmasterUser = "WarmasterUser";
    public const string WarmasterViewer = "WarmasterViewer";

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
