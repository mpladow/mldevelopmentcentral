namespace MldevDashboard.Api.Common;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Viewer = "Viewer";

    public static readonly string[] All =
    [
        Admin,
        User,
        Viewer
    ];
}
