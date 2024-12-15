using AggregationCRS.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace AggregationCRS.SQLClient
{
    public class DBRepositoryCache<TEntity> : IDBRepository<TEntity> where TEntity : class, IDBEntity
    {
        private readonly ISqlClientDatabase _sqlClientDatabase;
        private readonly IMemoryCache _cache;

        public DBRepositoryCache(ISqlClientDatabase sqlClientDatabase, IMemoryCache cache)
        {
            _sqlClientDatabase = sqlClientDatabase;
            _cache = cache;
        }

        public async Task<TEntity> GetAsync(string rowKey)
        {

            var key = $"{typeof(TEntity).Name}.{rowKey}";

            if (!_cache.TryGetValue(key, out TEntity result))
            {
                //Data not in cache
                result = await _sqlClientDatabase.GetAsync<TEntity>(rowKey);

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    // Keep in cache for this time, reset time if accessed.

                    //.SetSlidingExpiration(TimeSpan.FromSeconds(60));

                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(180)
                };

                // Save data in cache.
                _cache.Set(key, result, cacheEntryOptions);

            }

            return result;

        }

        public async Task<List<TEntity>> GetLikeAsync(string rowKeyPrefix, int limit = 0)
        {

            var key = $"{typeof(TEntity).Name}.{rowKeyPrefix}";

            if (!_cache.TryGetValue(key, out List<TEntity> result))
            {
                //Data not in cache
                result = await _sqlClientDatabase.GetLikeAsync<TEntity>(rowKeyPrefix, limit);

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    // Keep in cache for this time, reset time if accessed.

                    //.SetSlidingExpiration(TimeSpan.FromSeconds(60));

                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(180)
                };

                // Save data in cache.
                _cache.Set(key, result, cacheEntryOptions);

            }

            return result;

        }

        public async Task<List<TEntity>> QueryAsync(string sql)
        {

            var key = $"{typeof(TEntity).Name}.{sql.GetHashCode()}";

            if (!_cache.TryGetValue(key, out List<TEntity> result))
            {
                //Data not in cache
                result = await _sqlClientDatabase.QueryAsync<TEntity>(sql);

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    // Keep in cache for this time, reset time if accessed.

                    //.SetSlidingExpiration(TimeSpan.FromSeconds(60));

                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(180)
                };

                // Save data in cache.
                _cache.Set(key, result, cacheEntryOptions);

            }


            return result;

        }

    }
}
