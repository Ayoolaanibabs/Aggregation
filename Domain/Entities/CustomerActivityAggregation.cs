using AggregationCRS.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.Domain.Entities
{
    [Index(nameof(CustomerNumber)), Index(nameof(AggregationMonth)), Index(nameof(Currency)), Index(nameof(ActivityId))]
    public class CustomerActivityAggregation
    {
        [Key]
        [NotNull]
        public string EntityKey { get; set; } = string.Empty;

        public string CustomerNumber { get; set; } = string.Empty;

        public string AccountNumber { set; get; } = string.Empty;

        public string Currency { set; get; } = string.Empty;

        public string ActivityId { get; set; } = string.Empty;
        public string ActivityCode { get; set; } = string.Empty;
        public string GlobalActivityCode { get; set; } = string.Empty;

        public string ActivityName { get; set; } = string.Empty;

        public DateTime LastUpdated { set; get; }
        public decimal TotalAmount { get; set; } = decimal.Zero;
        public DateOnly AggregationMonth { get; set; }

        public int TotalCount { get; set; } = 0;


    }
}
