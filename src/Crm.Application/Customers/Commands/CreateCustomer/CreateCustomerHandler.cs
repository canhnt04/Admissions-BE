using Crm.Application.Common.Interfaces;
using Crm.Application.Events;
using Crm.Domain.Entities;
using MassTransit;
using MediatR;

namespace Crm.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly ICrmDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateCustomerHandler(ICrmDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Mobile = request.Mobile,
                StudentId = request.StudentId,
                Source = request.Source,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                Address = request.Address,
                TrainingSystem = request.TrainingSystem,
                EducationLevel = request.EducationLevel,
                PlaceOfBirth = request.PlaceOfBirth,
                LatestSchool = request.LatestSchool,
                OnlineMessageMobile = request.OnlineMessageMobile,
                Ethnic = request.Ethnic,
                SchoolAddress = request.SchoolAddress,
                UserIdByOa = request.UserIdByOa,
                ParentMobile = request.ParentMobile,
                CCCD = request.CCCD,
                CCCDIssueDate = request.CCCDIssueDate,
                FatherName = request.FatherName,
                MotherName = request.MotherName,
                GraduationYear = request.GraduationYear,
                CreatedBy = request.CreatedBy,
                Status = CustomerStatus.Interest,
                SaleStatus = Domain.Entities.SaleStatus.Cold,
                CreationDate = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow,
            };

            _context.Customers.Add(customer);

            // Ghi AuditLog
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = Domain.Entities.Action.Insert,
                Detail = $"Tạo khách hàng mới: {customer.Name} - {customer.Mobile}",
                RecordId = customer.Id,
                RecordDesc = customer.Name,
                RecordEntity = RecordEntity.Customer,
                CreationDate = DateTime.UtcNow,
                UserId = request.CreatedBy,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Publish event để AutoAssignmentConsumer tự động giao lead
            await _publishEndpoint.Publish(new CustomerCreatedEvent
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                Mobile = customer.Mobile,
                TrainingSystem = request.TrainingSystem,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
            }, cancellationToken);

            return customer.Id;
        }
    }
}
