using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
namespace LeadAssignment.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task NotifyLeadAssignedAsync(Guid recipientId, Guid customerId, string customerName, CancellationToken cancellationToken = default);
        Task NotifySlaWarningAsync(Guid recipientId, Guid customerId, string customerName, DateTime deadline, CancellationToken cancellationToken = default);
        Task NotifySlaViolationAsync(Guid recipientId, Guid customerId, string customerName, CancellationToken cancellationToken = default);
        Task NotifyLeadReassignedAsync(Guid recipientId, Guid customerId, string customerName, string reason, CancellationToken cancellationToken = default);
    }
}
