using System.ComponentModel;
namespace Shared.Contracts.Enums 
{ 
    public enum TrainingSystem 
    { 
        [Description("Sơ cấp")] ShortTerm = 1, 
        [Description("Chính quy")] Formal = 2, 
        [Description("Lái xe")] Driving = 3
    } 
}
