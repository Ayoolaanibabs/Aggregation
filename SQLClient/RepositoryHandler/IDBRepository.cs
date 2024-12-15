using AggregationCRS.Domain;

namespace AggregationCRS.SQLClient
{
    public interface IDBRepository<TEntity> where TEntity:class, IDBEntity
    {
        

        Task<TEntity> GetAsync(string rowKey);

        Task<List<TEntity>> GetLikeAsync(string rowKeyPrefix, int limit = 0);

        Task<List<TEntity>> QueryAsync(string sql);

    }
}
