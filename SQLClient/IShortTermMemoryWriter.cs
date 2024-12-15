using AggregationCRS.Domain;
using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Managers;

namespace AggregationCRS.SQLClient
{
    public interface IShortTermMemoryWriter
    {
        public string GetShortTermMemory(string shortTermMemory, ActivityMessageETO message);
        public string GetGLShortTermMemory(string shortTermMemory, StreamComputation message);

    }
}
