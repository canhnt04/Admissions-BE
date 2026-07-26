namespace Crm.Application.Common.Interfaces
{
    /// <summary>
    /// Service gửi thông báo cho NV tư vấn (in-app, email, Zalo...).
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Gửi thông báo khi NV được giao lead mới.
        /// </summary>
        Task NotifyLeadAssignedAsync(Guid recipientId, Guid customerId, string customerName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gửi cảnh báo SLA sắp hết hạn (ví dụ: còn 5 phút).
        /// </summary>
        Task NotifySlaWarningAsync(Guid recipientId, Guid customerId, string customerName, DateTime deadline, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gửi thông báo SLA bị vi phạm — lead bị thu hồi.
        /// </summary>
        Task NotifySlaViolationAsync(Guid recipientId, Guid customerId, string customerName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gửi thông báo lead được giao lại (cho NV mới nhận lead).
        /// </summary>
        Task NotifyLeadReassignedAsync(Guid recipientId, Guid customerId, string customerName, string reason, CancellationToken cancellationToken = default);
    }
}
