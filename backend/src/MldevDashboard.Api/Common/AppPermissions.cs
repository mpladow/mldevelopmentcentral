namespace MldevDashboard.Api.Common;

public static class AppPermissions
{
    public const string GlobalMenuAccounts = "global.menu.accounts";
    public const string GlobalMenuSystems = "global.menu.systems";
    public const string GlobalMenuRoles = "global.menu.roles";
    public const string GlobalAccountsManage = "global.accounts.manage";
    public const string GlobalSystemsManage = "global.systems.manage";
    public const string GlobalRolesManage = "global.roles.manage";
    public const string WarmasterMenuFactions = "warmaster.menu.factions";
    public const string WarmasterMenuUnits = "warmaster.menu.units";
    public const string WarmasterFactionsView = "warmaster.factions.view";
    public const string WarmasterUnitsView = "warmaster.units.view";

    public static readonly PermissionDefinition[] Global =
    [
        new(GlobalMenuAccounts, "Accounts menu", "Menus"),
        new(GlobalMenuSystems, "Systems menu", "Menus"),
        new(GlobalMenuRoles, "Roles menu", "Menus"),
        new(GlobalAccountsManage, "Manage accounts", "Administration"),
        new(GlobalSystemsManage, "Manage systems", "Administration"),
        new(GlobalRolesManage, "Manage roles", "Administration")
    ];

    public static readonly string[] GlobalAdminMinimum =
    [
        GlobalMenuAccounts,
        GlobalMenuSystems,
        GlobalMenuRoles,
        GlobalAccountsManage,
        GlobalSystemsManage,
        GlobalRolesManage
    ];

    public static readonly PermissionDefinition[] Warmaster =
    [
        new(WarmasterMenuFactions, "Factions menu", "Menus"),
        new(WarmasterMenuUnits, "Units menu", "Menus"),
        new(WarmasterFactionsView, "View factions", "Factions"),
        new(WarmasterUnitsView, "View units", "Units")
    ];

    public static readonly string[] WarmasterAdminDefault =
    [
        WarmasterMenuFactions,
        WarmasterMenuUnits,
        WarmasterFactionsView,
        WarmasterUnitsView
    ];
}

public sealed record PermissionDefinition(
    string Key,
    string DisplayName,
    string Category);
