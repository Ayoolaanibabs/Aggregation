using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AggregationCRS.Domain.Entities
{
    [Index(nameof(GLAccount))]
    public class GLComputationSummary
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
    }
}
