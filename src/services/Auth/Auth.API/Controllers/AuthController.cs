using Auth.Application.Features.Authentication.Commands.AssignUser;
using Auth.Application.Features.Authentication.Commands.Login;
using Auth.Application.Features.Authentication.Commands.Register;
using Auth.Application.Features.Authentication.Queries;
using Auth.Application.Features.Authentication.Queries.GetProfile;
using Auth.Application.Features.Authentication.Queries.GetUserById;
using Auth.Application.Features.Authentication.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Exceptions;
using Auth.Domain.Errors;
using System.Security.Claims;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterResponse>> Register(RegisterCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(LoginCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);    
        }

        [HttpPost("assign-user")]
        public async Task<ActionResult<AssignUserResponse>> AssignRole(AssignUserCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new UnauthorizedException(AuthErrors.InvalidCredentials);

            var query = new GetProfileQuery { UserId = userId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var response = await _mediator.Send(new GetUsersQuery());
            return Ok(response);
        }

        [HttpGet("users/{id:guid}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var response = await _mediator.Send(new GetUserByIdQuery { UserId = id });
            return Ok(response);
        }
    }
}
