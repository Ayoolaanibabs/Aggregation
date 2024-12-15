using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.Domain.Repository
{
    public interface IBackgroundJobWriter
    {
        Task ClearActivityShortTermMemoryTable();
        Task ClearStreamShortTermMemoryTable();
        Task ClearDailyGLSummaryShortTermMemoryTable();
        Task ClearCustomerActivityShortTermMemoryTable();
        Task ClearCustomerStreamShortTermMemoryTable();
        Task ClearConcessionUtilizationSummaryShortTermMemoryTable();
        Task ClearGLSummaryShortTermMemoryTable();
        Task ClearCustomerActivityAggregationTable();
        Task ClearCustomerStreamAggregationTable();

    }
}
