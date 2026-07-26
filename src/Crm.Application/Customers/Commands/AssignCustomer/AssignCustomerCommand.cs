using MediatR;

namespace Crm.Application.Customers.Commands.AssignCustomer
{
    /// <summary>
    /// Command giao lead thủ công cho 1 NV cụ thể (bởi Admin/Manager).
    /// </summary>
    public class AssignCustomerCommand : IRequest<bool>
    {
        /// <summary>
        /// ID khách hàng cần giao
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// ID NV tư vấn được giao
        /// </summary>
        public Guid AssigneeId { get; set; }

        /// <summary>
        /// ID người thực hiện giao (Admin/Manager) — lấy từ JWT token
        /// </summary>
        public Guid AssignedById { get; set; }

        /// <summary>
        /// Ghi chú lý do giao
        /// </summary>
        public string? Note { get; set; }
    }
}
