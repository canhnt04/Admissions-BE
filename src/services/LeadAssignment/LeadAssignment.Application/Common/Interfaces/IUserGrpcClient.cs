using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Application.Common.Interfaces
{
    public interface IUserGrpcClient
    {
        Task<Dictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
