using LeadAssignment.Application.Assignments.Commands.AssignPendingLeads;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using Shared.Contracts.Events.Customer;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Enums;

namespace LeadAssignment.Infrastructure.Consumers
{
    /// <summary>
    /// MassTransit Consumer: xử lý CustomerCreatedEvent.
    /// Tự động đưa lead vào hàng chờ Pending và trigger Assignment Engine.
    /// </summary>
    public class AutoAssignmentConsumer : IConsumer<CustomerCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ICustomerCareStatusRepository _customerCareStatusRepository;
        private readonly IAssignmentDbContext _context;
        private readonly ILogger<AutoAssignmentConsumer> _logger;

        public AutoAssignmentConsumer(
            IMediator mediator, 
            ICustomerCareStatusRepository customerCareStatusRepository,
            IAssignmentDbContext context,
            ILogger<AutoAssignmentConsumer> logger)
        {
            _mediator = mediator;
            _customerCareStatusRepository = customerCareStatusRepository;
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CustomerCreatedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "Nhận CustomerCreatedEvent: KH {CustomerName} ({CustomerId}), nhánh {TrainingSystem}",
                msg.CustomerName, msg.CustomerId, msg.TrainingSystem);

            // Add pending lead
            _customerCareStatusRepository.Add(new CustomerCareStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = msg.CustomerId,
                CustomerName = msg.CustomerName,
                TrainingSystem = msg.TrainingSystem ?? TrainingSystem.ShortTerm,
                AssigneeId = null, // Pending
                StatusDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(context.CancellationToken);

            // Trigger Assignment Engine
            var result = await _mediator.Send(new AssignPendingLeadsCommand
            {
                TrainingSystem = msg.TrainingSystem ?? TrainingSystem.ShortTerm,
            }, context.CancellationToken);

            if (result.Data)
            {
                _logger.LogInformation(
                    "Đã chạy Assignment Engine thành công cho nhánh {TrainingSystem}.",
                    msg.TrainingSystem);
            }
            else
            {
                _logger.LogWarning(
                    "Assignment Engine chạy cho nhánh {TrainingSystem} không có NV nào rảnh, lead vẫn đang Pending.",
                    msg.TrainingSystem);
            }
        }
    }
}
