using LeadAssignment.Application.Assignments.Commands.AutoAssign;
using Shared.Contracts.Events.Customer;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LeadAssignment.Infrastructure.Consumers
{
    /// <summary>
    /// MassTransit Consumer: xử lý CustomerCreatedEvent.
    /// Tự động giao lead mới cho NV tiếp theo trong queue (Round-Robin).
    /// </summary>
    public class AutoAssignmentConsumer : IConsumer<CustomerCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AutoAssignmentConsumer> _logger;

        public AutoAssignmentConsumer(IMediator mediator, ILogger<AutoAssignmentConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CustomerCreatedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "Nhận CustomerCreatedEvent: KH {CustomerName} ({CustomerId}), nhánh {TrainingSystem}",
                msg.CustomerName, msg.CustomerId, msg.TrainingSystem);

            var result = await _mediator.Send(new AutoAssignCommand
            {
                CustomerId = msg.CustomerId,
                TrainingSystem = msg.TrainingSystem,
            }, context.CancellationToken);

            if (result.Data.HasValue)
            {
                _logger.LogInformation(
                    "Auto-assigned KH {CustomerId} cho NV {AssigneeId}",
                    msg.CustomerId, result.Data.Value);
            }
            else
            {
                _logger.LogWarning(
                    "Không thể auto-assign KH {CustomerId}. Queue rỗng hoặc tất cả NV đã đầy tải.",
                    msg.CustomerId);
            }
        }
    }
}
