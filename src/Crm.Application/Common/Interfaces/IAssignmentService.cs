using Crm.Domain.Entities;

namespace Crm.Application.Common.Interfaces
{
    /// <summary>
    /// Service giao khách tự động theo Round-Robin queue.
    /// Mỗi nhánh đào tạo (Formal/ShortTerm/Driving) có queue riêng.
    /// </summary>
    public interface IAssignmentService
    {
        /// <summary>
        /// Tự động giao 1 KH mới cho NV tiếp theo trong queue (Round-Robin).
        /// </summary>
        /// <param name="customerId">ID khách hàng cần giao</param>
        /// <param name="trainingSystem">Nhánh đào tạo</param>
        /// <param name="cancellationToken"></param>
        /// <returns>ID của NV được giao, null nếu queue rỗng hoặc tất cả NV đã đầy tải</returns>
        Task<Guid?> AutoAssignAsync(Guid customerId, TrainingSystem trainingSystem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Giao lead thủ công cho 1 NV cụ thể (bởi Admin/Manager).
        /// </summary>
        Task ManualAssignAsync(Guid customerId, Guid assigneeId, Guid assignedById, string? note = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Thu hồi lead từ NV vi phạm SLA và giao cho NV tiếp theo trong queue.
        /// </summary>
        /// <param name="customerId">ID khách hàng</param>
        /// <param name="violatedAssigneeId">ID NV bị thu hồi</param>
        /// <param name="cancellationToken"></param>
        /// <returns>ID của NV mới được giao, null nếu không tìm được NV phù hợp</returns>
        Task<Guid?> ReassignAfterSlaViolationAsync(Guid customerId, Guid violatedAssigneeId, CancellationToken cancellationToken = default);
    }
}
