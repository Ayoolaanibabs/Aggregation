using AggregationCRS.Domain;
using AggregationCRS.Domain.Entities;
using AggregationCRS.Domain.Managers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AggregationCRS.SQLClient
{
    public class ShortTermMemoryWriter : IShortTermMemoryWriter
    {
        private readonly IConfiguration _configuration;
        int shortTermMemoryLength = 3;

        public ShortTermMemoryWriter(IConfiguration configuration)
        {
            _configuration = configuration;
            shortTermMemoryLength = _configuration.GetValue<int>("ShortTermMemoryLength");
        }

        public string GetShortTermMemory(string shortTermMemory, ActivityMessageETO message)
        {
            List<ShortTermMemory> shortTermMemories = JsonConvert.DeserializeObject<List<ShortTermMemory>>(shortTermMemory);

            if (shortTermMemories == null) shortTermMemories = new List<ShortTermMemory>();
            
            if (true)
            {
                var cutOffDate = message.ActivityDate.AddDays(-shortTermMemoryLength);
                shortTermMemories = shortTermMemories.Where(x => x.ActDate >= DateOnly.FromDateTime(cutOffDate)).ToList();
                shortTermMemories.Add(new ShortTermMemory { ActDate = DateOnly.FromDateTime(message.ActivityDate), RefNum = message.ActivityReference });

            } else
            {
                var cutOffDate = message.StreamDate.AddDays(-shortTermMemoryLength);
                shortTermMemories = shortTermMemories.Where(x => x.ActDate >= DateOnly.FromDateTime(cutOffDate)).ToList();
                shortTermMemories.Add(new ShortTermMemory { ActDate = DateOnly.FromDateTime(message.StreamDate), RefNum = message.StreamReference });
            }
            return JsonConvert.SerializeObject(shortTermMemories);
        }

        public string GetGLShortTermMemory(string shortTermMemory, StreamComputation message)
        {
            List<ShortTermMemory> shortTermMemories = JsonConvert.DeserializeObject<List<ShortTermMemory>>(shortTermMemory);

            if (shortTermMemories == null) shortTermMemories = new List<ShortTermMemory>();

            var cutOffDate = message.StreamDate.Value.AddDays(-shortTermMemoryLength);
            shortTermMemories = shortTermMemories.Where(x => x.ActDate >= DateOnly.FromDateTime(cutOffDate)).ToList();
            shortTermMemories.Add(new ShortTermMemory { ActDate = DateOnly.FromDateTime(message.StreamDate.Value), RefNum = message.StreamReference });
            return JsonConvert.SerializeObject(shortTermMemories);
        }
    }
}
