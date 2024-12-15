using AggregationCRS.Domain;

namespace AggregationCRS.SQLClient
{
    public class DBRepository<TEntity> : IDBRepository<TEntity> where TEntity : class, IDBEntity
    {
        private readonly ISqlClientDatabase _sqlDatabase;

        public DBRepository(ISqlClientDatabase sqlDatabase)
        {
            _sqlDatabase = sqlDatabase;
        }

        public Task<TEntity> GetAsync(string rowKey)
        {
            var result = _sqlDatabase.GetAsync<TEntity>(rowKey);
            return result;
        }

        public Task<List<TEntity>> GetLikeAsync(string rowKeyPrefix, int limit = 0)
        {
            var result = _sqlDatabase.GetLikeAsync<TEntity>(rowKeyPrefix, limit);
            return result;

        }

        public Task<List<TEntity>> QueryAsync(string sql)
        {
            var result = _sqlDatabase.QueryAsync<TEntity>(sql);
            return result;

        }

    }
}
