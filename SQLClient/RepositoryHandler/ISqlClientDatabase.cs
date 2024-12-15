
using AggregationCRS.Domain;

namespace AggregationCRS.SQLClient
{
    public interface ISqlClientDatabase
    {
        Task<TEntity> GetAsync<TEntity>(string rowKey) where TEntity : IDBEntity;
        Task<List<TEntity>> GetLikeAsync<TEntity>(string rowKeyPrefix, int limit) where TEntity : IDBEntity;
        Task<List<TEntity>> QueryAsync<TEntity>(string sql) where TEntity : IDBEntity;
        Task ExecuteNonQueryAsync(string query);
    }
}