using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Repository;
using KafkaFlow;
using Newtonsoft.Json;

namespace AggregationCRS.Domain.Managers
{
    public class GLSummaryHandleAggregation : IMessageMiddleware
    {
        private readonly IGLSummaryWriter _gLSummaryWriter;

        public GLSummaryHandleAggregation(IGLSummaryWriter gLSummaryWriter)
        {
            _gLSummaryWriter = gLSummaryWriter;
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

            var writeGLSummary = _gLSummaryWriter.WriteGLSummaryDataAsync(messages);
            var writeDailyGLSummary = _gLSummaryWriter.WriteDailyGLSummaryDataAsync(messages);

            Task.WaitAll(writeGLSummary, writeDailyGLSummary);


            Console.WriteLine("Topic: {0} | Partition: {1} | Offset: {2}",context.ConsumerContext.Topic,context.ConsumerContext.Partition,context.ConsumerContext.Offset);
        }
    }
}
