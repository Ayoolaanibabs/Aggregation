using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AggregationCRS.Domain.Entities
{
    [Index(nameof(CustomerNumber))]
    public class ActivityAggregation
    {
        [Key]
        [NotNull]
        public string EntityKey { get; set; } = string.Empty;

        public string CustomerNumber { get; set; } = string.Empty;

        public string AccountNumber { set; get; } = string.Empty;

        public string Currency { set;get; } = string.Empty;

        public string ActivityId { get; set; } = string.Empty;
        public string ActivityCode { get; set; } = string.Empty;
        public string GlobalActivityCode { get; set; } = string.Empty;

        public string ActivityName { get; set; } = string.Empty;
        //public string ShortTerm { get; set; } = string.Empty;

        public string DailyActivitySummary { set; get; } = string.Empty;

        //public double MonthCount { set; get; }

        //public double MonthAvg { set; get; }

        //public double MonthMax { set; get; }

        //public double MonthMin { set; get; } 
        
        //public double MonthSum { set; get; }         

        //public DateOnly MonthYear { set; get; }

        public DateTime LastUpdated {  set; get; }

    }


    public class ActivitySummary
    {
        public DateOnly ActDate { get; set; }
        public int Cnt { get; set; }
        public double Vol { set; get; } 
        public double VolMax { set; get; } 
        public double VolMin { set; get; }
        public double VolAvg { set; get; }
    }
}
