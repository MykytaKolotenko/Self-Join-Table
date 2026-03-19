using Microsoft.Data.SqlClient;

public class DatabaseInitializer : IHostedService
{
    private const string CONNECTION_STRING = "DefaultConnection";
    private const string INDEXES_NAME = "IX_Employee_ManagerID_Enable";

    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = config.GetConnectionString(CONNECTION_STRING)!;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsureIndexesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task EnsureIndexesAsync()
    {
        await using SqlConnection conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        bool exists = await IndexExistsAsync(conn, INDEXES_NAME);

        if (exists)
        {
            await using SqlCommand dropCmd = new SqlCommand($@"
                DROP INDEX [{INDEXES_NAME}] ON [dbo].[Employee]", conn);
            await dropCmd.ExecuteNonQueryAsync();
        }

        await using SqlCommand createCmd = new SqlCommand($@"
            CREATE NONCLUSTERED INDEX [{INDEXES_NAME}] 
            ON [Employee] (ManagerID, Enable) 
            INCLUDE (ID, Name)", conn);

        await createCmd.ExecuteNonQueryAsync();
    }

    private async Task<bool> IndexExistsAsync(SqlConnection conn, string indexName)
    {
        await using SqlCommand cmd = new SqlCommand(@"
            SELECT COUNT(*) FROM sys.indexes 
            WHERE name = @indexName AND object_id = OBJECT_ID('Employee')", conn);

        cmd.Parameters.AddWithValue("@indexName", indexName);
        int count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }
}
