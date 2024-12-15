using System.Reflection;

namespace AggregationCRS.SQLClient
{
    public class DBSchema
    {
        public string RowKeyName { get; set; }

        public string TableName { get; set; }

        public Dictionary<string, PhoenixColumn> Fields { get; set; }
    }

    public class PhoenixColumn
    {
        public string ColumnName { get; set; }

        public int ColumnOrder { get; set; }

        public PropertyInfo Field { get; set; }
    }
}
