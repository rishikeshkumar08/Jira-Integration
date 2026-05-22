using System.Data;
using Microsoft.Data.SqlClient;

namespace Api.Utilities;

public sealed class SqlHelper
{
    private SqlHelper() { }

    private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
    {
        if (commandParameters == null) return;

        foreach (var p in commandParameters)
        {
            if (p == null) continue;

            if ((p.Direction == ParameterDirection.InputOutput ||
                 p.Direction == ParameterDirection.Input) &&
                p.Value == null)
            {
                p.Value = DBNull.Value;
            }
            command.Parameters.Add(p);
        }
    }

    private static void PrepareCommand(
        SqlCommand command,
        SqlConnection connection,
        SqlTransaction? transaction,
        CommandType commandType,
        string commandText,
        SqlParameter[]? commandParameters,
        out bool mustCloseConnection)
    {
        if (connection.State != ConnectionState.Open)
        {
            mustCloseConnection = true;
            connection.Open();
        }
        else
        {
            mustCloseConnection = false;
        }

        command.Connection = connection;
        command.CommandTimeout = 120;
        command.CommandText = commandText;
        command.CommandType = commandType;

        if (transaction != null)
            command.Transaction = transaction;

        if (commandParameters != null)
            AttachParameters(command, commandParameters);
    }

    public static DataSet ExecuteDataset(
        string connectionString,
        CommandType commandType,
        string commandText,
        params SqlParameter[] commandParameters)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        return ExecuteDataset(connection, commandType, commandText, commandParameters);
    }

    public static DataSet ExecuteDataset(
        SqlConnection connection,
        CommandType commandType,
        string commandText,
        params SqlParameter[] commandParameters)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        using var cmd = new SqlCommand();
        PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters, out var mustCloseConnection);

        using var da = new SqlDataAdapter(cmd);
        var ds = new DataSet();
        da.Fill(ds);

        cmd.Parameters.Clear();
        if (mustCloseConnection)
            connection.Close();

        return ds;
    }
}