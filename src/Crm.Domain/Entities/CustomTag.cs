using System;

namespace Crm.Domain.Entities
{
    public class CustomTag
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// Nhánh đào tạo mà tag này thuộc về (nullable = áp dụng chung)
        /// </summary>
        public TrainingSystem? TrainingSystem { get; set; }

        public bool IsActive { get; set; }
    }
}
