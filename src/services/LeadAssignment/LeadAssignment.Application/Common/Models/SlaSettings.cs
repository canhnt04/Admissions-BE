using System;

namespace LeadAssignment.Application.Common.Models
{
    public class SlaSettings
    {
        public int SlaDeadlineMinutes { get; set; } = 30;
        public int AdminSlaDeadlineMinutes { get; set; } = 120;
        public int MaxSlaMultiplier { get; set; } = 4;
        public Guid DefaultManagerId { get; set; }
        public System.Collections.Generic.Dictionary<string, Guid> Managers { get; set; } = new();
    }
}
