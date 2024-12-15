using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Repository;
using KafkaFlow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregationCRS.Domain.Managers
{
    public class HandleStreamAggregation: IMessageMiddleware
    {
        private readonly IDataWriter _dataWriter;


        public HandleStreamAggregation(IDataWriter dataWriter)
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

            var writeStreamTask = _dataWriter.WriteStreamDataAsync(messages);
            var writeCustomerStreamTask = _dataWriter.WriteCustomerStreamDataAsync(messages);

            Task.WaitAll(writeStreamTask, writeCustomerStreamTask);

            Console.WriteLine("Topic: {0} | Partition: {1} | Offset: {2}", context.ConsumerContext.Topic, context.ConsumerContext.Partition, context.ConsumerContext.Offset);
        }
    }
}
