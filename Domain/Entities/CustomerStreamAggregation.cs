using AggregationCRS.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.Domain.Entities
{
    [Index(nameof(CustomerNumber)), Index(nameof(AggregationMonth)), Index(nameof(StreamId)), Index(nameof(StreamCurrency)), Index(nameof(StreamType)), Index(nameof(ActivityId))]
    public class CustomerStreamAggregation
    {
        [Key]
        [NotNull]
        public string EntityKey { get; set; } = string.Empty;

        public string CustomerNumber { get; set; } = string.Empty;

        public string AccountNumber { set; get; } = string.Empty;

        public string StreamId { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; } = decimal.Zero;

        public StreamType StreamType { get; set; }

        public DateTime LastUpdated { get; set; }

        public string StreamCurrency { set; get; } = string.Empty;
        public string StreamName { get; set; } = string.Empty;


        public string ActivityId { get; set; } = string.Empty;

        public DateOnly AggregationMonth { get; set; }
        public int TotalCount { get; set; } = 0;

    }
}
