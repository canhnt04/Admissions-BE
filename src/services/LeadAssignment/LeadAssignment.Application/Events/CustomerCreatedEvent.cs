using LeadAssignment.Domain.Entities;

namespace LeadAssignment.Application.Events
{
    /// <summary>
    /// Event phát ra khi có KH mới được tạo bên các service nghiệp vụ.
    /// LeadAssignment consumer sẽ lắng nghe event này để tự động giao lead.
    /// </summary>
    public class CustomerCreatedEvent
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public TrainingSystem TrainingSystem { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
