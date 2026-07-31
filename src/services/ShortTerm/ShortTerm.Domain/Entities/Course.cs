using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortTerm.Domain.Entities
{
    public class Course
    {
        public Guid Id { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool? IsStarted { get; set; }
        public bool IsGraduated { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 1=SCNH, 2=TCCQ, 3=CDCQ, 4=DHLT, 5=CD9, 6=Driving, 7=Technical
        /// </summary>
        public CourseCategory? Category { get; set; }

        // Khóa ngoại liên kết với CustomTag
        public Guid? CustomTagId { get; set; }
        public virtual CustomTag CustomTag { get; set; }
    }
}
