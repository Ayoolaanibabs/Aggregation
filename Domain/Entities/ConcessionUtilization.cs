using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AggregationCRS.Domain.Entities
{
    [Index(nameof(GLAccount)), Index(nameof(CustomerNumber)), Index(nameof(AccountNumber))]
    public class ConcessionUtilization
    {
        [Key]
        public string EntityKey { get; set; }

        [MaxLength(100)]    
        public string GLAccount { get; set; } = string.Empty;

        [MaxLength(100)]
        public string AccountNumber { set; get; }  = string.Empty;
        
        [MaxLength(100)]
        public string CustomerNumber { set; get; }  = string.Empty;
        public string Branch { set; get; } = string.Empty;
        public string StreamId { set; get; } = string.Empty;
        public string StreamReference { set; get; } = string.Empty;
        public string ActivityReference { set; get; } = string.Empty;
        public string Currency { set; get; } = string.Empty;
        [MaxLength(100)]
        public string ConcessionId { set; get; }
        public DateOnly UtilizationDate { set; get; } = new DateOnly();
        public double UtilizedAmount { set; get; }
    }


    [Index(nameof(GLAccount)), Index(nameof(CustomerNumber)), Index(nameof(AccountNumber))]
    public class ConcessionUtilizationSummary
    {
        [Key]
        public string EntityKey { get; set; }

        [MaxLength(100)]
        public string GLAccount { get; set; } = string.Empty;

        [MaxLength(100)]
        public string AccountNumber { set; get; } = string.Empty;

        [MaxLength(100)]
        public string CustomerNumber { set; get; } = string.Empty;
        public string Branch { set; get; } = string.Empty;
        public string StreamId { set; get; } = string.Empty;
        public DateOnly MonthYear { set; get; }
        public string Currency { set; get; } = string.Empty;
        //[MaxLength(100)]
        //public string ConcessionId { set; get; } = string.Empty;
        public DateTime LastUpdate { set; get; } = new DateTime();
        public double UtilizedAmount { set; get; }
        //public string ShortMemory { set; get; } = string.Empty;
    }

}
