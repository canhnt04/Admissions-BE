using System.ComponentModel;
using Shared.Contracts.Converters;

namespace Shared.Contracts.Enums 
{ 
    [TypeConverter(typeof(EnumDescriptionTypeConverter<TrainingSystem>))]
    public enum TrainingSystem 
    { 
        [Description("Sơ cấp")] ShortTerm = 1, 
        [Description("Chính quy")] Formal = 2, 
        [Description("Lái xe")] Driving = 3
    } 
}
