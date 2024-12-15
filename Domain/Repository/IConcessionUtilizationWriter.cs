using AggregationCRS.Domain.Entities;

namespace AggregationCRS.Domain.Repository
{
    public interface IConcessionUtilizationWriter
    {
        Task WriteConcessionUtilizationDataAsync(List<StreamComputation> messageETOs);
        Task WriteConcessionUtilizationSummaryDataAsync(List<StreamComputation> messageETOs);
    }
}