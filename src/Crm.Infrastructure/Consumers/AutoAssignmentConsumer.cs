using Crm.Application.Common.Interfaces;
using Crm.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Consumers
{
    /// <summary>
    /// MassTransit Consumer: xử lý CustomerCreatedEvent.
    /// Tự động giao lead mới cho NV tiếp theo trong queue (Round-Robin).
    /// </summary>
    public class AutoAssignmentConsumer : IConsumer<CustomerCreatedEvent>
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<AutoAssignmentConsumer> _logger;

        public AutoAssignmentConsumer(IAssignmentService assignmentService, ILogger<AutoAssignmentConsumer> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CustomerCreatedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "Nhận CustomerCreatedEvent: KH {CustomerName} ({CustomerId}), nhánh {TrainingSystem}",
                msg.CustomerName, msg.CustomerId, msg.TrainingSystem);

            var assigneeId = await _assignmentService.AutoAssignAsync(
                msg.CustomerId, msg.TrainingSystem, context.CancellationToken);

            if (assigneeId.HasValue)
            {
                _logger.LogInformation(
                    "Auto-assigned KH {CustomerId} cho NV {AssigneeId}",
                    msg.CustomerId, assigneeId.Value);
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
