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
        // 1) Open a raw admin connection
        await using var adminConn = new NpgsqlConnection(ConfigurationHelper.GetConfigurationValueByKey("ConnectionStrings:MasterConnection"));
        await adminConn.OpenAsync();

        // 2) Create the role if needed
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

        // 3) Create the database if needed
        var checkDbCmd = new NpgsqlCommand(
            $"SELECT 1 FROM pg_database WHERE datname = '{tenantName}';", adminConn);

        bool dbExists = false;
        
        await using (var reader = await checkDbCmd.ExecuteReaderAsync())
            dbExists = reader.Read();

        if (!dbExists)
        {
            // This must be outside any transaction
            var createDbCmd = new NpgsqlCommand(
                $"CREATE DATABASE \"{tenantName}\" OWNER \"{tenantName}\";", adminConn);
            await createDbCmd.ExecuteNonQueryAsync();
        }

        // 4) Grant privileges
        var grantCmd = new NpgsqlCommand(
            $"GRANT ALL PRIVILEGES ON DATABASE \"{tenantName}\" TO \"{tenantName}\";", adminConn);
        await grantCmd.ExecuteNonQueryAsync();

        // 5) Now connect to the *new* database to set up schema privileges.
        var tenantConnectionString =
            $"Host=localhost;Database={tenantName};Username={tenantName};Password={dbPassword};";
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

        // Run migrations on the new tenant's database;
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(tenantConn);
        await using var context = new ApplicationDbContext(builder.Options);
        await context.Database.MigrateAsync();
    }
}