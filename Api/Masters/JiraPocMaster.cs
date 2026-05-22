using System.Data;
using Api.Models;
using Api.Utilities;
using Microsoft.Data.SqlClient;

namespace Api.Masters;

public class JiraPocMaster
{
    private readonly string _connString;

    public JiraPocMaster(IConfiguration configuration)
    {
        _connString = configuration.GetConnectionString("connstr")
            ?? throw new InvalidOperationException("ConnectionStrings:connstr is missing.");
    }

    public JiraPocTicket Upsert(
        string jiraKey,
        string title,
        string? description,
        string priority,
        string status)
    {
        var ds = SqlHelper.ExecuteDataset(
            _connString,
            CommandType.StoredProcedure,
            "dbo.JiraPoc_Upsert",
            new SqlParameter("@JiraKey", jiraKey),
            new SqlParameter("@Title", title),
            new SqlParameter("@Description", (object?)description ?? DBNull.Value),
            new SqlParameter("@Priority", priority),
            new SqlParameter("@Status", status));

        return MapFirst(ds) ?? throw new InvalidOperationException("Upsert returned no row.");
    }

    public JiraPocTicket? GetByJiraKey(string jiraKey)
    {
        var ds = SqlHelper.ExecuteDataset(
            _connString,
            CommandType.StoredProcedure,
            "dbo.JiraPoc_GetByJiraKey",
            new SqlParameter("@JiraKey", jiraKey));

        return MapFirst(ds);
    }

    public JiraPocTicket? GetById(int id)
    {
        var ds = SqlHelper.ExecuteDataset(
            _connString,
            CommandType.StoredProcedure,
            "dbo.JiraPoc_GetById",
            new SqlParameter("@Id", id));

        return MapFirst(ds);
    }

    public IReadOnlyList<JiraPocTicket> ListAll()
    {
        var ds = SqlHelper.ExecuteDataset(
            _connString,
            CommandType.StoredProcedure,
            "dbo.JiraPoc_ListAll");

        return MapAll(ds);
    }

    private static JiraPocTicket? MapFirst(DataSet ds)
    {
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return ds.Tables[0].Rows[0].ToObject<JiraPocTicket>();
    }

    private static IReadOnlyList<JiraPocTicket> MapAll(DataSet ds)
    {
        if (ds.Tables.Count == 0)
            return Array.Empty<JiraPocTicket>();

        return ds.Tables[0].ToList<JiraPocTicket>().ToList();
    }
}