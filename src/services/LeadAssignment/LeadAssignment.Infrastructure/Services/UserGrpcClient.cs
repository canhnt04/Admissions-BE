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

        public async Task<Dictionary<Guid, (string FullName, string Email)>> GetUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var distinctIds = userIds.Distinct().Select(id => id.ToString()).ToList();
            if (!distinctIds.Any())
                return new Dictionary<Guid, (string FullName, string Email)>();

            var request = new GetUsersRequest();
            request.UserIds.AddRange(distinctIds);

            try
            {
                var response = await _client.GetUsersByIdsAsync(request, cancellationToken: cancellationToken);
                var result = new Dictionary<Guid, (string FullName, string Email)>();
                foreach (var kvp in response.UserMap)
                {
                    if (Guid.TryParse(kvp.Key, out var guid))
                    {
                        result[guid] = (kvp.Value.FullName, kvp.Value.Email);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GRPC ERROR] {ex.Message}");
                Console.WriteLine(ex.ToString());
                return new Dictionary<Guid, (string FullName, string Email)>();
            }   
        }
    }
}
