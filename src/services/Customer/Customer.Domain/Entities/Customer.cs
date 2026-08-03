using System;
using Shared.Contracts.Enums;
using Customer.Domain.Enums;

namespace Customer.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public int CustomerNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public Source? Source { get; set; }
        public TrainingSystem? TrainingSystem { get; set; }
        
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public string Address { get; set; }
        
        public CustomerStatus? Status { get; set; }
        public DateTime? CreationDate { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
