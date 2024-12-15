using AggregationCRS.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AggregationCRS.SQLClient
{
    public class DBSchemaRegistry:IDBSchemaRegistry
    {
        private Dictionary<string, DBSchema> schemaRegistry;


        public DBSchemaRegistry()
        {
            schemaRegistry = new Dictionary<string, DBSchema>();
            RegisterAllSchema();
        }

        public DBSchema GetSchema(string schemaName)
        {
            return schemaRegistry.GetValueOrDefault(schemaName);
            
        }

        private void RegisterAllSchema()
        {
            //Register all the known entities here

            List<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => typeof(IDBEntity).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .ToList();

            foreach(var t in types)
            {
                var schema = new DBSchema();
                var T = Activator.CreateInstance(t);

                var tableName = GetTableName(t);

                var rowKey = GetRowKey(t);

                schema.TableName = tableName;
                schema.RowKeyName = rowKey;

                schemaRegistry.Add(t.Name, schema);
            }

        }

        private string GetTableName(Type entity)
        {
            var tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(entity, typeof(TableAttribute));

            var tableName = tableAttribute?.Name ??
                throw new NullReferenceException(ExceptionMessages.MissingTableName);

            return tableName;
        }

        private string GetRowKey(Type entity)
        {
            var properties = entity.GetProperties();

            string rowKey = "";

            foreach (var property in properties)
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    var attributType = attribute.GetType();

                    if (typeof(KeyAttribute).IsAssignableFrom(attributType))
                    {
                        var columnAttribute = (ColumnAttribute)Attribute.GetCustomAttribute(property, typeof(ColumnAttribute));
                        rowKey = columnAttribute?.Name ?? throw new NullReferenceException(ExceptionMessages.MissingRowKeyName);
                    }

                    continue;

                }

            }

            if(string.IsNullOrEmpty(rowKey))
            {
                throw new NullReferenceException(ExceptionMessages.MissingRowKeyName);
            }

            return rowKey;
        }
    }
}
