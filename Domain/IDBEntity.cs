namespace AggregationCRS.Domain
{
    public interface IDBEntity
    {
        public Dictionary<string, string> Result { get; set; }
        public void MapEntity();
    }
}
