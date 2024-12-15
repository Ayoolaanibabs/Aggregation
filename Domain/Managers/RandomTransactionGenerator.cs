using AggregationCRS.Domain.Entities;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop.Implementation;
using Newtonsoft.Json;

namespace AggregationCRS.Domain.Managers
{/// <summary>
/// This class is for test purposes only and should not be run in real life/production
/// </summary>
    public class RandomTransactionGenerator : BackgroundService
    {
        const string topic = "activity-topic-new";
        private readonly IProducerAccessor _producerAccessor;
        const string producerName = "transactionProducer";
        private  IMessageProducer _producer;    


        public RandomTransactionGenerator(IProducerAccessor producerAccessor)
        {
            _producerAccessor = producerAccessor;
            _producer = _producerAccessor.GetProducer(producerName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Guid LCEstablishmentId = Guid.Parse("00ded136-6414-43eb-b1e2-cd6801b4eda4");
            Guid LCStreamEstablishmentId = Guid.Parse("303f9878-34bb-4bc4-a83a-8510317c395d");

            string account1 = "0011223453";
            string account2 = "9988775577";
            Random random = new Random();

            bool test = true;
            int i = 1;
            /*while (test)
            {
                var seedNumber = random.NextDouble();

                var input = new ActivityMessageETO()
                {
                    AccountNumber = i % 2 == 0 ? account1 : account2,
                    ActivityCode = "LC_Est",
                    ActivityCurrency = "NGN",
                    ActivityDate = DateTime.Now.AddDays(-seedNumber*10),
                    ActivityId = LCEstablishmentId.ToString(),
                    ActivityName = "LC Establishment",
                    StreamTypeId = "0",
                    GlobalActivityCode = "GB_LC_Est",
                    ActivityVolume = seedNumber * i * 1000,
                    ActualFlowAmount = i % 2 == 0 ? seedNumber * i * 5 : seedNumber * i * 10,
                    ConceededFlowAmount = i % 2 == 0 ? i * 5 : 0,
                    ConceededFlowAmountACY = i % 2 == 0 ? i * 5 : 0,
                    CustomerNumber = "00992726",
                    EventId = Guid.NewGuid().ToString(),
                    ExpectedFlowAmount = seedNumber * i * 10,
                    StreamCode = "LC_Est_Comm",
                    StreamCurrency = "NGN",
                    GLAccount = "PAL0011223445",
                    Branch = "001",
                    StreamReference = Guid.NewGuid().ToString(),
                    StreamDate = DateTime.Now.AddDays(-seedNumber * 10).AddMinutes(2),
                    StreamId = LCStreamEstablishmentId.ToString(),
                    VarianceFlowAmount = 0
                };


                await _producer.ProduceAsync(topic, input.AccountNumber,JsonConvert.SerializeObject(input));

                i++;

                if (i % 20 == 0) 
                Console.WriteLine($"Message sent!{i}");
            }*/
                
            
        }
    }
}
