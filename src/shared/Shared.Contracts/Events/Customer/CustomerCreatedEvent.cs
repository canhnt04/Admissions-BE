using System;
using Shared.Contracts.Enums;

namespace Shared.Contracts.Events.Customer
{
    public record CustomerCreatedEvent(
        Guid CustomerId,
        string CustomerName,
        string Mobile,
        TrainingSystem? TrainingSystem
    );
}
