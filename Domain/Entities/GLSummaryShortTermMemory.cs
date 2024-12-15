﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.Domain.Entities
{
    public class GLSummaryShortTermMemory
    {
        [Key]
        [NotNull]
        public string Ref { get; set; } = string.Empty;
        public DateTime ActDate { get; set; }
    }
}
