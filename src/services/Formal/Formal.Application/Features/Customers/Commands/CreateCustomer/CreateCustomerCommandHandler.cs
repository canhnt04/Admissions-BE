using Formal.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Formal.Application.Common.Interfaces;
using Formal.Domain.Entities;
using MediatR;
using Shared.Common;
using MassTransit;
using Formal.Application.Events;

namespace Formal.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IFormalDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateCustomerCommandHandler(ICustomerRepository customerRepository, IFormalDbContext context, IPublishEndpoint publishEndpoint)
        {
            _customerRepository = customerRepository;
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Formal.Domain.Entities.Customer
            {
                Id = Guid.NewGuid(),
                // CustomerCode = request.CustomerId, (CustomerNumber is auto-generated int)
                Name = request.FullName,
                Mobile = request.Mobile,
                Source = (Source)request.Source,
                CreationDate = DateTime.UtcNow
            };

            _customerRepository.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            // Publish event cho Auto-Assignment
            await _publishEndpoint.Publish(new CustomerCreatedEvent
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                Mobile = customer.Mobile ?? "",
                TrainingSystem = Shared.Contracts.Enums.TrainingSystem.Formal,
                CreatedBy = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            return Result<Guid>.Success(customer.Id);
        }
    }
}
