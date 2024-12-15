using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.Domain.Entities
{
    [Index(nameof(ActDate))]
    public class ActivityShortTermMemory
    {
        [Key]
        [NotNull]
        public string Ref { get; set; } = string.Empty;
        public DateTime ActDate { get; set; }
    }
}
