using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using MediatR;
using Shared.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LeadAssignment.Application.Assignments.Commands.CreateContactEvidence
{
    public class CreateContactEvidenceCommand : IRequest<Result<bool>>
    {
        public Guid CustomerId { get; set; }
        public LeadStatus Status { get; set; }
        public FollowStatus FollowStatus { get; set; }
        public string? Note { get; set; }
        public Guid AssigneeId { get; set; }
    }

    public class CreateContactEvidenceCommandHandler : IRequestHandler<CreateContactEvidenceCommand, Result<bool>>
    {
        private readonly ICustomerCareStatusRepository _customerCareStatusRepository;
        private readonly IAssignmentDbContext _context;
        private readonly ILogger<CreateContactEvidenceCommandHandler> _logger;

        public CreateContactEvidenceCommandHandler(
            ICustomerCareStatusRepository customerCareStatusRepository,
            IAssignmentDbContext context,
            ILogger<CreateContactEvidenceCommandHandler> logger)
        {
            _customerCareStatusRepository = customerCareStatusRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(CreateContactEvidenceCommand request, CancellationToken cancellationToken)
        {
            var status = await _customerCareStatusRepository.GetLatestActiveAsync(request.CustomerId, cancellationToken);
            if (status == null)
            {
                return Result<bool>.Failure(new Error(404, "Evidence.NotFound", "Không tìm thấy thông tin phân công của khách hàng này."));
            }

            status.Status = request.Status;
            status.FollowStatus = request.FollowStatus;
            status.Note = request.Note;
            status.StatusDate = Shared.Common.Helpers.TimeHelper.VietnamNow;
            status.ReportDate = Shared.Common.Helpers.TimeHelper.VietnamNow;

            _customerCareStatusRepository.Update(status);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Nhân viên {AssigneeId} ghi nhận bằng chứng thành công cho khách hàng {CustomerId}. Trạng thái mới: {Status}", request.AssigneeId, request.CustomerId, request.Status);

            return Result<bool>.Success(true);
        }
    }
}
