using ShortTerm.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using ShortTerm.Application.Common.Interfaces;
using ShortTerm.Domain.Entities;
using MediatR;
using Shared.Common;
using MassTransit;
using Shared.Contracts.Events.Customer;

namespace ShortTerm.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IShortTermDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateCustomerCommandHandler(ICustomerRepository customerRepository, IShortTermDbContext context, IPublishEndpoint publishEndpoint)
        {
            _customerRepository = customerRepository;
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new ShortTerm.Domain.Entities.Customer
            {
                Id = Guid.NewGuid(),
                Name = request.FullName,
                Mobile = request.Mobile ?? "",
                Email = "",
                Source = (Source)request.Source,
                CreationDate = DateTime.UtcNow,
                TrainingSystem = Shared.Contracts.Enums.TrainingSystem.ShortTerm,
                Gender = "",
                Address = "",
                PlaceOfBirth = "",
                LatestSchool = "",
                OnlineMessageMobile = "",
                Ethnic = "",
                SchoolAddress = "",
                UserIdByOa = "",
                ParentMobile = "",
                CCCD = "",
                FatherName = "",
                MotherName = ""
            };

            _customerRepository.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            // Publish event cho Auto-Assignment
            await _publishEndpoint.Publish(new CustomerCreatedEvent(
                customer.Id,
                customer.Name,
                customer.Mobile,
                Shared.Contracts.Enums.TrainingSystem.ShortTerm
            ), cancellationToken);

            return Result<Guid>.Success(customer.Id);
        }
    }
}
