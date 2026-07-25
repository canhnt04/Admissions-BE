using Crm.Domain.Entities;

namespace Crm.Application.Auth
{
    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string IdentificationNumber { get; set; }
        public Role Role { get; set; }
    }

    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class AssignRoleRequest
    {
        public Guid UserId { get; set; }
        public Role Role { get; set; }
        public Guid? TeamId { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string FullName { get; set; }
        public Role Role { get; set; }
    }
}
