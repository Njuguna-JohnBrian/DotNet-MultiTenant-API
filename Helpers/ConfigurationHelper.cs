namespace MultitenancyApp.Helpers;

public class ConfigurationHelper
{
    private static readonly IConfigurationRoot Configuration;
    private static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;

    static ConfigurationHelper()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{Env}.json", true);

        Configuration = builder.Build();
    }


    public static string? GetConfigurationValueByKey(string key)
    {
        return Configuration[key];
    }
}