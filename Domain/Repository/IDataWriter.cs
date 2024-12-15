using AggregationCRS.Domain.Entities;

namespace AggregationCRS.Domain.Repository
{
    public interface IDataWriter
    {
        public Task WriteActivityDataAsync(List<ActivityMessageETO> messages);
        public Task WriteCustomerActivityDataAsync(List<ActivityMessageETO> messages);
        public Task ShrinkDbLogFile();
        public Task WriteStreamDataAsync(List<ActivityMessageETO> messages);
        public Task WriteCustomerStreamDataAsync(List<ActivityMessageETO> messages);
    }
}
