
using System;
namespace AggregationCRS.SQLClient
{
    public interface IDBSchemaRegistry
    {
        DBSchema GetSchema(string schemaName);
    }
}
