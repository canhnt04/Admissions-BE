using Grpc.Net.Client;
using LeadAssignment.Application.Common.Interfaces;
using Shared.Protos.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Infrastructure.Services
{
    public class UserGrpcClient : IUserGrpcClient
    {
        private readonly UserService.UserServiceClient _client;

        public UserGrpcClient(UserService.UserServiceClient client)
        {
            _client = client;
        }

        public async Task<Dictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var distinctIds = userIds.Distinct().Select(id => id.ToString()).ToList();
            if (!distinctIds.Any())
                return new Dictionary<Guid, string>();

            var request = new GetUsersRequest();
            request.UserIds.AddRange(distinctIds);

            try
            {
                var response = await _client.GetUsersByIdsAsync(request, cancellationToken: cancellationToken);
                var result = new Dictionary<Guid, string>();
                foreach (var kvp in response.UserMap)
                {
                    if (Guid.TryParse(kvp.Key, out var guid))
                    {
                        result[guid] = kvp.Value;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                // Nếu gRPC fail, trả về dictionary rỗng (fallback)
                return new Dictionary<Guid, string>();
            }
        }
    }
}
