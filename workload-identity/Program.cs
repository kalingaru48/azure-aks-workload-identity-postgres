using Npgsql;
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Core;

class Script
{
    private static string Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? string.Empty;
    private static string User = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? string.Empty;
    private static string Database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE")?? string.Empty;

    static async Task Main(string[] args)
    {
        Console.Out.WriteLine("Getting access token from Azure AD...");

        var credential = new DefaultAzureCredential();
        var tokenRequestContext = new TokenRequestContext(new[] { "https://ossrdbms-aad.database.windows.net/.default" });
        var accessToken = await credential.GetTokenAsync(tokenRequestContext);

        Console.Out.WriteLine("Access token received: {0}", accessToken.Token);
        
        string connString = String.Format(
            "Server={0}; User Id={1}; Database={2}; Port={3}; Password={4}; SSLMode=Prefer",
            Host,
            User,
            Database,
            5432,
            accessToken.Token);

        using (var conn = new NpgsqlConnection(connString))
        {
            Console.Out.WriteLine("Opening connection using access token...");
            conn.Open();

            using (var command = new NpgsqlCommand("SELECT version()", conn))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\nConnected!\n\nPostgres version: {0}", reader.GetString(0));
                }
            }
        }
    }
}
