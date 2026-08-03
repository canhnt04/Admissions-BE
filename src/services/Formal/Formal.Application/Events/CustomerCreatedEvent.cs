using Formal.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Formal.Domain.Entities;

namespace Formal.Application.Events
{
    /// <summary>
    /// Event phát ra khi có KH mới được tạo.
    /// AutoAssignmentConsumer sẽ lắng nghe event này để tự động giao lead.
    /// </summary>
    public class CustomerCreatedEvent
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public TrainingSystem TrainingSystem { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
