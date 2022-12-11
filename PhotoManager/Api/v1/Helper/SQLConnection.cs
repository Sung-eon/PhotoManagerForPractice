namespace PhotoManager.Api.v1.Helper;

public interface ISqlConnection
{
    string ConnectionString();
}

public record class MSSQL(string host, string db, string user, string password): ISqlConnection
{
    public string ConnectionString()
    {
        return $"Server={host};Database={db};User ID={user};Password={password}";
    }
}