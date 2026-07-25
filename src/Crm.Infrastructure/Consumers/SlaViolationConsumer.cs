using Crm.Application.Common.Interfaces;
using Crm.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Consumers
{
    /// <summary>
    /// MassTransit Consumer: xử lý SlaViolationEvent.
    /// Thu hồi lead từ NV vi phạm SLA và giao cho NV tiếp theo trong queue.
    /// </summary>
    public class SlaViolationConsumer : IConsumer<SlaViolationEvent>
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<SlaViolationConsumer> _logger;

        public SlaViolationConsumer(IAssignmentService assignmentService, ILogger<SlaViolationConsumer> logger)
        {
            _assignmentService = assignmentService;
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

            var newAssigneeId = await _assignmentService.ReassignAfterSlaViolationAsync(
                msg.CustomerId, msg.ViolatedAssigneeId, context.CancellationToken);

            if (newAssigneeId.HasValue)
            {
                _logger.LogInformation(
                    "SLA Reassign thành công: KH {CustomerId} giao cho NV {NewAssigneeId}",
                    msg.CustomerId, newAssigneeId.Value);
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
