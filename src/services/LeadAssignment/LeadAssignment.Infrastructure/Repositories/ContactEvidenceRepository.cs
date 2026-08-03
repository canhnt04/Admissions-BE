using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Infrastructure.Data;
using Shared.Common.Repositories;

namespace LeadAssignment.Infrastructure.Repositories;

public class ContactEvidenceRepository : GenericRepository<ContactEvidence, AssignmentDbContext>, IContactEvidenceRepository
{
    public ContactEvidenceRepository(AssignmentDbContext dbContext) : base(dbContext)
    {
    }
}
