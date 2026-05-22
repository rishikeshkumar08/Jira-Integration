using System.Data;
using System.Reflection;

namespace Api.Utilities;

public static class DataRowExtensions
{
    public static IList<T> ToList<T>(this DataTable table) where T : new()
    {
        var properties = typeof(T).GetProperties().ToList();
        var result = new List<T>();

        foreach (DataRow row in table.Rows)
            result.Add(CreateItemFromRow<T>(row, properties));

        return result;
    }

    public static T ToObject<T>(this DataRow row) where T : new()
    {
        var properties = typeof(T).GetProperties().ToList();
        return CreateItemFromRow<T>(row, properties);
    }

    private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
    {
        var item = new T();
        foreach (var property in properties)
        {
            if (!row.Table.Columns.Contains(property.Name) || row[property.Name] == DBNull.Value)
                continue;

            if (property.PropertyType == typeof(string))
                property.SetValue(item, Convert.ToString(row[property.Name]));
            else
                property.SetValue(item, row[property.Name]);
        }
        return item;
    }
}