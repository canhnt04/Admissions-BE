using LeadAssignment.Domain.Entities;

namespace LeadAssignment.Application.Common.Interfaces
{
    /// <summary>
    /// Service giao khách tự động theo Round-Robin queue.
    /// </summary>
    public interface IAssignmentService
    {
        Task<Guid?> AutoAssignAsync(Guid customerId, TrainingSystem trainingSystem, CancellationToken cancellationToken = default);
        Task ManualAssignAsync(Guid customerId, Guid assigneeId, Guid assignedById, string? note = null, CancellationToken cancellationToken = default);
        Task<Guid?> ReassignAfterSlaViolationAsync(Guid customerId, Guid violatedAssigneeId, CancellationToken cancellationToken = default);
    }
}
