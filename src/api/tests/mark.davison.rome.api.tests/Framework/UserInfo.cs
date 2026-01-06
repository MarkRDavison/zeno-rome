namespace mark.davison.rome.api.tests.Framework;

public sealed record UserInfo(
    string Email,
    string Provider,
    string Sub,
    string Name,
    Guid UserId,
    Guid TenantId,
    string[] UserRoles);