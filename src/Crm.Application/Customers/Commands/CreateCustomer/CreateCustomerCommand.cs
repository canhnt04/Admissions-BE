using Crm.Domain.Entities;
using MediatR;

namespace Crm.Application.Customers.Commands.CreateCustomer
{
    /// <summary>
    /// Command tạo khách hàng mới.
    /// Sau khi tạo, hệ thống sẽ publish CustomerCreatedEvent để auto-assign lead.
    /// </summary>
    public class CreateCustomerCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string? StudentId { get; set; }
        public Source? Source { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public TrainingSystem TrainingSystem { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? LatestSchool { get; set; }
        public string? OnlineMessageMobile { get; set; }
        public string? Ethnic { get; set; }
        public string? SchoolAddress { get; set; }
        public string? UserIdByOa { get; set; }
        public string? ParentMobile { get; set; }
        public string? CCCD { get; set; }
        public DateTime? CCCDIssueDate { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public int? GraduationYear { get; set; }

        /// <summary>
        /// ID user đang tạo KH (lấy từ JWT token)
        /// </summary>
        public Guid CreatedBy { get; set; }
    }
}
