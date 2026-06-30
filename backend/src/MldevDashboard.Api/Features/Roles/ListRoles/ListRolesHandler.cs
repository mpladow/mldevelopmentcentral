using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Roles.ListRoles;

public sealed class ListRolesHandler
{
    public string[] Handle()
    {
        return ApplicationRoles.All;
    }
}
