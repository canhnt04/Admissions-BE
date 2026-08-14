namespace LeadAssignment.Application.Assignments.Queries.GetDashboardSummary
{
    public class DashboardSummaryDto
    {
        public KpiSummary Kpis { get; set; } = new();
        public BranchSummary Branches { get; set; } = new();
    }

    public class KpiSummary
    {
        public int TotalLeads { get; set; }
        public int ActiveSla { get; set; }
        public int OverdueSla { get; set; }
        public int UnassignedLeads { get; set; }
    }

    public class BranchSummary
    {
        public int Formal { get; set; }
        public int Driving { get; set; }
        public int ShortTerm { get; set; }
    }
}
