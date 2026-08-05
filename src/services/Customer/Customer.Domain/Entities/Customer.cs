using System;
using Shared.Contracts.Enums;
using Customer.Domain.Enums;

namespace Customer.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public int CustomerNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        
        public string? StudentId { get; set; }
        
        public Source? Source { get; set; }
        
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateTime { get; set; }
        
        public Guid CreatedBy { get; set; }
        public Guid? Assignee { get; set; }
        
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        
        public CustomerStatus? Status { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public EquivalentDegree? EquivalentDegree { get; set; }
        public SaleStatus? SaleStatus { get; set; }
        public TrainingSystem? TrainingSystem { get; set; }
        
        public string? PlaceOfBirth { get; set; }
        public string? LatestSchool { get; set; }
        public string? OnlineMessageMobile { get; set; }
        public string? Ethnic { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string? SchoolAddress { get; set; }
        public string? UserIdByOa { get; set; }
        public string? ParentMobile { get; set; }
        public string? CCCD { get; set; }
        public DateTime? CCCDIssueDate { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public LeadStatus? FinalStatus { get; set; }
        public int? GraduationYear { get; set; }
        public Enrollment? Enrollment { get; set; }
    }
}
