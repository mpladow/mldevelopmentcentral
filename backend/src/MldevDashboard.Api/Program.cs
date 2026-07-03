using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MldevDashboard.Api.Features.Global.Accounts;
using MldevDashboard.Api.Features.Global.Accounts.CreateAccount;
using MldevDashboard.Api.Features.Global.Accounts.ListAccounts;
using MldevDashboard.Api.Features.Global.Accounts.UpdateAccount;
using MldevDashboard.Api.Features.Auth;
using MldevDashboard.Api.Features.Auth.GetCurrentUser;
using MldevDashboard.Api.Features.Auth.Login;
using MldevDashboard.Api.Features.Global.Roles;
using MldevDashboard.Api.Features.Global.Roles.CreatePermission;
using MldevDashboard.Api.Features.Global.Roles.CreateRole;
using MldevDashboard.Api.Features.Global.Roles.ListRoles;
using MldevDashboard.Api.Features.Global.Roles.ListPermissions;
using MldevDashboard.Api.Features.Global.Roles.UpdateRole;
using MldevDashboard.Api.Features.Global.Systems;
using MldevDashboard.Api.Features.Global.Systems.CreateSystem;
using MldevDashboard.Api.Features.Global.Systems.DeleteSystem;
using MldevDashboard.Api.Features.Global.Systems.GetSystemTheme;
using MldevDashboard.Api.Features.Global.Systems.ListAvailableSystems;
using MldevDashboard.Api.Features.Global.Systems.ListSystems;
using MldevDashboard.Api.Features.Global.Systems.UpdateSystem;
using MldevDashboard.Api.Features.Global.Systems.UpdateSystemTheme;
using MldevDashboard.Api.Features.Warmaster.Factions;
using MldevDashboard.Api.Features.Warmaster.Factions.ListFactions;
using MldevDashboard.Api.Features.Warmaster.Units;
using MldevDashboard.Api.Features.Warmaster.Units.ListUnits;
using MldevDashboard.Api.Identity;
using MldevDashboard.Infrastructure;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AppAuthorizationService>();
builder.Services.AddScoped<ListAccountsHandler>();
builder.Services.AddScoped<CreateAccountHandler>();
builder.Services.AddScoped<UpdateAccountHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<GetCurrentUserHandler>();
builder.Services.AddScoped<ListRolesHandler>();
builder.Services.AddScoped<ListPermissionsHandler>();
builder.Services.AddScoped<CreatePermissionHandler>();
builder.Services.AddScoped<CreateRoleHandler>();
builder.Services.AddScoped<UpdateRoleHandler>();
builder.Services.AddScoped<ListAvailableSystemsHandler>();
builder.Services.AddScoped<ListSystemsHandler>();
builder.Services.AddScoped<CreateSystemHandler>();
builder.Services.AddScoped<UpdateSystemHandler>();
builder.Services.AddScoped<DeleteSystemHandler>();
builder.Services.AddScoped<GetSystemThemeHandler>();
builder.Services.AddScoped<UpdateSystemThemeHandler>();
builder.Services.AddScoped<ListFactionsHandler>();
builder.Services.AddScoped<ListUnitsHandler>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException("JWT signing key is not configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

await app.Services.SeedIdentityAsync(app.Configuration);

app.MapGet("/", () => Results.Ok(new
{
    name = "ML Dev Dashboard API",
    status = "Ready"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapAuthEndpoints();
app.MapAccountEndpoints();
app.MapRoleEndpoints();
app.MapSystemEndpoints();
app.MapFactionEndpoints();
app.MapUnitEndpoints();

app.Run();

public partial class Program
{
}
