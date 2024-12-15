using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.Domain.Entities
{
    [Index(nameof(GLAccount)), Index(nameof(SummaryDate))]
    public class DailyGLComputationSummary
    {
        [Key]
        public string EntityKey { get; set; }

        [MaxLength(100)]
        public string GLAccount { get; set; } = string.Empty;

        public string Branch { set; get; } = string.Empty;
        public string StreamId { set; get; } = string.Empty;

        public string Currency { set; get; } = string.Empty;
        public double ActualFlow { set; get; }
        public double ExpectedFlow { set; get; }
        public double? Difference { set; get; }
        //public string ShortTerm { set; get; } = string.Empty;
        public DateTime LastUpdate { set; get; }
        public DateOnly SummaryDate { set; get; }
    }
}
