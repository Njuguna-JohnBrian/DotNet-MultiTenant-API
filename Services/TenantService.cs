using Microsoft.EntityFrameworkCore;
using MultitenancyApp.DatabaseContext;
using MultitenancyApp.Helpers;
using MultitenancyApp.Interfaces;
using Npgsql;

namespace MultitenancyApp.Services;

public class TenantService : ITenantService
{
    public async Task CreateTenantDatabaseAsync(string tenantName, string dbPassword)
    {
        // Open admin connection
        await using var adminConn = new NpgsqlConnection(ConfigurationHelper.GetConfigurationValueByKey("ConnectionStrings:MasterConnection"));
        await adminConn.OpenAsync();

      
        var checkRoleCmd = new NpgsqlCommand(
            $"SELECT 1 FROM pg_roles WHERE rolname = '{tenantName}'", adminConn);
        
        bool roleExists = false;
        
        await using (var reader = await checkRoleCmd.ExecuteReaderAsync())
            roleExists = reader.Read();

        if (!roleExists)
        {
            var createRoleCmd = new NpgsqlCommand(
                $"CREATE ROLE \"{tenantName}\" WITH LOGIN PASSWORD '{dbPassword}';", adminConn);
            await createRoleCmd.ExecuteNonQueryAsync();
        }

        //Check if tenant database exists, of not create
        var checkDbCmd = new NpgsqlCommand(
            $"SELECT 1 FROM pg_database WHERE datname = '{tenantName}';", adminConn);

        bool dbExists = false;
        
        await using (var reader = await checkDbCmd.ExecuteReaderAsync())
            dbExists = reader.Read();

        if (!dbExists)
        {
            var createDbCmd = new NpgsqlCommand(
                $"CREATE DATABASE \"{tenantName}\" OWNER \"{tenantName}\";", adminConn);
            await createDbCmd.ExecuteNonQueryAsync();
        }

        //Grant tenant all privileges on created database
        var grantCmd = new NpgsqlCommand(
            $"GRANT ALL PRIVILEGES ON DATABASE \"{tenantName}\" TO \"{tenantName}\";", adminConn);
        await grantCmd.ExecuteNonQueryAsync();

        // Connect to tenant database and run migrations
        var tenantConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = ConfigurationHelper.GetConfigurationValueByKey("DbOptions:Host"),
            Port = int.Parse(ConfigurationHelper.GetConfigurationValueByKey("DbOptions:Port")!),
            Database = tenantName,
            Username = tenantName,
            Password = dbPassword
        }.ConnectionString;
        
        await using var tenantConn = new NpgsqlConnection(tenantConnectionString);
        await tenantConn.OpenAsync();

        await using var tenantCmd = tenantConn.CreateCommand();
        tenantCmd.CommandText = $@"
       GRANT ALL ON SCHEMA public TO ""{tenantName}"";
       GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ""{tenantName}"";
       ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO ""{tenantName}"";
       ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO ""{tenantName}"";
       ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON FUNCTIONS TO ""{tenantName}"";
    ";
        await tenantCmd.ExecuteNonQueryAsync();
        
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(tenantConn);
        await using var context = new ApplicationDbContext(builder.Options);
        await context.Database.MigrateAsync();
    }
}