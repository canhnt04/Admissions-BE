using Shared.Common;

namespace Auth.Domain.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(1201, "Auth.InvalidCredentials", "Sai thông tin đăng nhập.");
    public static readonly Error UserNotFound = new(1401, "Auth.UserNotFound", "Không tìm thấy người dùng.");
    public static readonly Error DuplicateUsername = new(1501, "Auth.DuplicateUsername", "Username already exists.");
    public static readonly Error TeamNotFound = new(1402, "Auth.TeamNotFound", "Không tìm thấy Team.");
}
