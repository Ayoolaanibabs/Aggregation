using AggregationCRS.Domain.Entities;

namespace AggregationCRS.Domain.Repository
{
    public interface IGLSummaryWriter
    {
        Task WriteGLSummaryDataAsync(List<StreamComputation> messageETOs);
        Task WriteDailyGLSummaryDataAsync(List<StreamComputation> messageETOs);
    }
}
