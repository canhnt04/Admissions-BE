using LeadAssignment.Application.Assignments.Commands.ReassignAfterSlaViolation;
using LeadAssignment.Application.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LeadAssignment.Infrastructure.Consumers
{
    /// <summary>
    /// MassTransit Consumer: xử lý SlaViolationEvent.
    /// Thu hồi lead từ NV vi phạm SLA và giao cho NV tiếp theo trong queue.
    /// </summary>
    public class SlaViolationConsumer : IConsumer<SlaViolationEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SlaViolationConsumer> _logger;

        public SlaViolationConsumer(IMediator mediator, ILogger<SlaViolationConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SlaViolationEvent> context)
        {
            var msg = context.Message;

            _logger.LogWarning(
                "SLA Violation: Thu hồi KH {CustomerName} ({CustomerId}) từ NV {ViolatedAssigneeName} ({ViolatedAssigneeId}). " +
                "Giao lúc: {AssignedAt}, Deadline: {Deadline}, Vi phạm lúc: {ViolatedAt}",
                msg.CustomerName, msg.CustomerId,
                msg.ViolatedAssigneeName, msg.ViolatedAssigneeId,
                msg.AssignedAt, msg.Deadline, msg.ViolatedAt);

            var result = await _mediator.Send(new ReassignAfterSlaViolationCommand
            {
                CustomerId = msg.CustomerId,
                ViolatedAssigneeId = msg.ViolatedAssigneeId,
            }, context.CancellationToken);

            if (result.Data.HasValue)
            {
                _logger.LogInformation(
                    "SLA Reassign thành công: KH {CustomerId} giao cho NV {NewAssigneeId}",
                    msg.CustomerId, result.Data.Value);
            }
            else
            {
                _logger.LogError(
                    "SLA Reassign thất bại: Không tìm được NV thay thế cho KH {CustomerId}",
                    msg.CustomerId);
            }
        }
    }
}
