using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AggregationCRS.Domain.Entities
{
    public class StreamAggregation
    {
        [Key]
        [NotNull]
        public string EntityKey { get; set; } = string.Empty;

        public string CustomerNumber { get; set; } = string.Empty;

        public string AccountNumber { set; get; } = string.Empty;

        public string StreamCurrency { set;get; } = string.Empty;

        public string StreamId { get; set; } = string.Empty;
        public string ActivityId { get; set; } = string.Empty;

        public string ActivityCode { get; set; } = string.Empty;
        public string StreamName { get; set; } = string.Empty;

        public string DailyStreamSummary { set; get; } = string.Empty;
        //public string ShortTerm { set; get; } = string.Empty;
        public DateTime LastUpdated {  set; get; } 

    }


    public class StreamSummary
    {
        public DateOnly FlowDt { get; set; }
        public int Cnt { get; set; }
        public double ActAmt { set; get; }
        public double ExpAmt { set; get; }
        public double ConcAmt { set; get; }
    }


}
