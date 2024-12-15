using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Repository;
using KafkaFlow;
using Newtonsoft.Json;

namespace AggregationCRS.Domain.Managers
{
    public class ConcessionUtilizationAggregation : IMessageMiddleware
    {
        private readonly IConcessionUtilizationWriter _concessionUtilizationWriter;

        public ConcessionUtilizationAggregation(IConcessionUtilizationWriter concessionUtilizationWriter)
        {
            _concessionUtilizationWriter = concessionUtilizationWriter;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var batch = context.GetMessagesBatch();

            if (batch.Count() == 0) { return; }

            List<StreamComputation> messages = new List<StreamComputation>();   

            foreach (var item in batch)
            {
                messages.Add(JsonConvert.DeserializeObject<StreamComputation>(item.Message.Value.ToString()));
            }

           var singleConcessionTask =  _concessionUtilizationWriter.WriteConcessionUtilizationDataAsync(messages);
           var summaryConcessionTask =  _concessionUtilizationWriter.WriteConcessionUtilizationSummaryDataAsync(messages);

            Task.WaitAll(singleConcessionTask, summaryConcessionTask);

            Console.WriteLine("Topic: {0} | Partition: {1} | Offset: {2}",context.ConsumerContext.Topic,context.ConsumerContext.Partition,context.ConsumerContext.Offset);
        }
    }
}
