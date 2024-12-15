using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Repository;
using KafkaFlow;
using Newtonsoft.Json;

namespace AggregationCRS.Domain.Managers
{
    public class HandleAggregation : IMessageMiddleware
    {
        private readonly IDataWriter _dataWriter;


        public HandleAggregation(IDataWriter dataWriter)
        {
            _dataWriter = dataWriter;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var batch = context.GetMessagesBatch();

            if (batch.Count() == 0) { return; }

            List<ActivityMessageETO> messages = new List<ActivityMessageETO>();   

            foreach (var item in batch)
            {
                messages.Add(JsonConvert.DeserializeObject<ActivityMessageETO>(item.Message.Value.ToString()));
            }

            var writeActivityTask =  _dataWriter.WriteActivityDataAsync(messages);
            var writeCustomerActivityTask = _dataWriter.WriteCustomerActivityDataAsync(messages);


            Task.WaitAll( writeActivityTask, writeCustomerActivityTask);

            Console.WriteLine("Topic: {0} | Partition: {1} | Offset: {2}",context.ConsumerContext.Topic,context.ConsumerContext.Partition,context.ConsumerContext.Offset);
        }
    }
}
