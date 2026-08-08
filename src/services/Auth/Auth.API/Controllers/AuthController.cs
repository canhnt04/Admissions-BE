using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Auth.Application.Features.Authentication.Commands.AssignUser;
using Auth.Application.Features.Authentication.Commands.RemoveUserTeam;
using Auth.Application.Features.Authentication.Commands.Login;
using Auth.Application.Features.Authentication.Commands.Register;
using Auth.Application.Features.Authentication.Queries;
using Auth.Application.Features.Authentication.Queries.GetProfile;
using Auth.Application.Features.Authentication.Queries.GetUserById;
using Auth.Application.Features.Authentication.Queries.GetUsers;
using Auth.Application.Features.Authentication.Queries.GetTeams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Exceptions;
using Auth.Domain.Errors;
using System.Security.Claims;

using Shared.Common.Controllers;

namespace Auth.API.Controllers
{
    /// <summary>
    /// Quản lý xác thực và người dùng
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterCommand command)
        {
            var response = await _mediator.Send(command);
            return HandleResult(response);
        }

        /// <summary>
        /// Đăng nhập 
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginCommand command)
        {
            var response = await _mediator.Send(command);
            return HandleResult(response);
        }

        /// <summary>
        /// Lấy danh sách toàn bộ RoleTeam (Admin)
        /// </summary>
        [HttpGet("teams")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetTeams()
        {
            var response = await _mediator.Send(new GetTeamsQuery());
            return HandleResult(response);
        }

        /// <summary>
        /// Cấp quyền Role or RoleTeam (Admin)
        /// </summary>
        [HttpPost("assign-user")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignRole(AssignUserCommand command)
        {
            var response = await _mediator.Send(command);
            return HandleResult(response);
        }

        /// <summary>
        /// Gỡ người dùng khỏi RoleTeam (Admin)
        /// </summary>
        [HttpPost("remove-team")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RemoveTeam(RemoveUserTeamCommand command)
        {
            var response = await _mediator.Send(command);
            return HandleResult(response);
        }

        /// <summary>
        /// Lấy thông tin cá nhân của người dùng đang đăng nhập 
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult> GetProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var query = new GetProfileQuery { UserId = userId };
            var response = await _mediator.Send(query);
            return HandleResult(response);
        }

        /// <summary>
        /// Lấy danh sách toàn bộ người dùng
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult> GetUsers()
        {
            var response = await _mediator.Send(new GetUsersQuery());
            return HandleResult(response);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID 
        /// </summary>
        [HttpGet("users/{id:guid}")]
        public async Task<ActionResult> GetUserById(Guid id)
        {
            var response = await _mediator.Send(new GetUserByIdQuery { UserId = id });
            return HandleResult(response);
        }
    }
}
