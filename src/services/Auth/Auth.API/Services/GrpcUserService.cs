using Grpc.Core;
using Shared.Protos.Users;
using Auth.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.API.Services
{
    public class GrpcUserService : UserService.UserServiceBase
    {
        private readonly IAuthDbContext _context;
        private readonly ILogger<GrpcUserService> _logger;

        public GrpcUserService(IAuthDbContext context, ILogger<GrpcUserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<GetUsersResponse> GetUsersByIds(GetUsersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Nhận yêu cầu gRPC lấy tên cho {Count} Users", request.UserIds.Count);

            var userIds = request.UserIds
                .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id.ToString(), u => new UserGrpcDto { FullName = u.FullName, Email = u.Email ?? string.Empty }, context.CancellationToken);

            var response = new GetUsersResponse();
            foreach (var kvp in users)
            {
                response.UserMap.Add(kvp.Key, kvp.Value);
            }

            return response;
        }
    }
}
