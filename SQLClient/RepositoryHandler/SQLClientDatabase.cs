using AggregationCRS.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Data;

namespace AggregationCRS.SQLClient
{
    public class SqlClientDatabase : ISqlClientDatabase
    {
        private readonly IDBSchemaRegistry _schemaRegistry;
        private readonly IConfiguration _config;

        public SqlClientDatabase(IDBSchemaRegistry schemaRegistry, IConfiguration config)
        {
            _schemaRegistry = schemaRegistry;
            _config = config;
        }

        private async Task<DataTable> ExecuteQuery(string sql)
        {
            try
            {
                var data = new DataTable();

                using (SqlConnection srcConn = new SqlConnection(_config.GetConnectionString("Default")))
                {
                    using (SqlCommand cmd = new(sql, srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }
                       
                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            cmd.CommandText = sql;
                            dataAdapter.Fill(data);
                        }                        
                    }
                    await srcConn.CloseAsync();
                }

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ExecuteNonQueryAsync(string query)
        {
            try
            {
                using (SqlConnection srcConn = new SqlConnection(_config.GetConnectionString("Default")))
                {
                    using (SqlCommand cmd = new(query, srcConn))
                    {
                        if (srcConn.State != ConnectionState.Open) { await srcConn.OpenAsync(); }
                        await cmd.ExecuteNonQueryAsync();
                    }
                    await srcConn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
            }
        }

        public async Task<TEntity> GetAsync<TEntity>(string rowKey) where TEntity : IDBEntity
        {
            if (rowKey == null)
            {
                throw new ArgumentNullException();
            }

            var phoenixSchema = GetSchema<TEntity>();
            var sql = GetQuery(phoenixSchema, rowKey);

            var resultResponse = await ExecuteQuery(sql);

            var result = resultResponse.Rows;

            var columns = resultResponse.Columns;


            if (result.Count <= 0)
            {
                //No result found

                return default;

            }

            //Get first item in list
            //TODO If result set is greater than 1 throw an exception.  considering it's practically impossible in hbase, is it necessary?
            var resultData = result[0];


            return MapEntity<TEntity>(columns, resultData);
        }

        public async Task<List<TEntity>> GetLikeAsync<TEntity>(string rowKeyPrefix, int limit) where TEntity : IDBEntity
        {
            if (rowKeyPrefix == null)
            {
                throw new ArgumentNullException();
            }

            var phoenixSchema = GetSchema<TEntity>();
            var sql = GetLikeQuery(phoenixSchema, rowKeyPrefix, limit);
            var resultResponse = await ExecuteQuery(sql);
            var results = resultResponse.Rows;
            var columns = resultResponse.Columns;

            if (results.Count <= 0)
            {
                //No result found
                return default;
            }

            var resultList = new List<TEntity>();

            foreach (DataRow result in results)
            {
                resultList.Add(MapEntity<TEntity>(columns, result, true));
            }

            return resultList;
        }

        public async Task<List<TEntity>> QueryAsync<TEntity>(string sql) where TEntity : IDBEntity
        {
            if (string.IsNullOrEmpty(sql.Trim()))
            {
                throw new ArgumentNullException();
            }

            var resultResponse = await ExecuteQuery(sql);

            var results = resultResponse.Rows;

            var columns = resultResponse.Columns;


            if (results.Count <= 0)
            {
                //No result found
                return default;
            }

            var resultList = new List<TEntity>();

            foreach (DataRow result in results)
            {
                resultList.Add(MapEntity<TEntity>(columns, result, true));
            }
            return resultList;
        }

        private TEntity MapEntity<TEntity>(DataColumnCollection columns, DataRow resultData, bool mapDictionary = true) where TEntity : IDBEntity
        {
            TEntity T = Activator.CreateInstance<TEntity>();
            var resultDict = new Dictionary<string, string>();
            foreach (DataColumn column in columns)
            {
                var value = resultData[column].ToString();
                resultDict.Add(column.ColumnName, value);
            }
            T.Result = resultDict;
            T.MapEntity();
            return T;
        }

        private string GetQuery(DBSchema schema, string rowKey)
        {
            return $"SELECT * FROM {schema.TableName} WHERE {schema.RowKeyName} = '{rowKey}'";
        }

        private string GetLikeQuery(DBSchema schema, string rowKeyPrefix, int limit)
        {
            if (limit > 0) return $"SELECT TOP {limit} * FROM {schema.TableName} WHERE {schema.RowKeyName} LIKE '{rowKeyPrefix}%' ";
            return $"SELECT * FROM {schema.TableName} WHERE {schema.RowKeyName} LIKE '{rowKeyPrefix}%'";
        }

        private DBSchema GetSchema<TEntity>() where TEntity : IDBEntity
        {
            var entity = typeof(TEntity);
            var schemaName = entity.Name;
            return _schemaRegistry.GetSchema(schemaName);
        }

    }
}
