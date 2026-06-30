namespace MldevDashboard.Api.Common;

public enum ApplicationResultStatus
{
    Success,
    Created,
    BadRequest,
    NotFound,
    Conflict,
    Unauthorized
}

public sealed record ApplicationResult<T>(
    ApplicationResultStatus Status,
    T? Value = default,
    string? Message = null,
    string[]? Errors = null)
{
    public static ApplicationResult<T> Success(T value) => new(ApplicationResultStatus.Success, value);

    public static ApplicationResult<T> Created(T value) => new(ApplicationResultStatus.Created, value);

    public static ApplicationResult<T> BadRequest(string message) => new(ApplicationResultStatus.BadRequest, Message: message);

    public static ApplicationResult<T> BadRequest(string[] errors) => new(ApplicationResultStatus.BadRequest, Errors: errors);

    public static ApplicationResult<T> NotFound(string message) => new(ApplicationResultStatus.NotFound, Message: message);

    public static ApplicationResult<T> Conflict(string message) => new(ApplicationResultStatus.Conflict, Message: message);

    public static ApplicationResult<T> Unauthorized() => new(ApplicationResultStatus.Unauthorized);
}
