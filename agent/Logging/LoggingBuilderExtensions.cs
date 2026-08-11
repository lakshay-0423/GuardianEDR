namespace Guardian.Agent.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder ConfigureGuardianLogging(
        this ILoggingBuilder logging,
        IConfiguration configuration)
    {
        logging.ClearProviders();
        logging.AddConfiguration(configuration.GetSection("Logging"));
        logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "O";
            options.UseUtcTimestamp = true;
        });

        return logging;
    }
}
