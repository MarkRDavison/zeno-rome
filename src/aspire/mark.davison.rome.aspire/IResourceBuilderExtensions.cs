using mark.davison.rome.shared.constants;
using Microsoft.Extensions.Configuration;

namespace mark.davison.rome.aspire;

public static class IResourceBuilderExtensions
{
    private const string _httpsEndpointName = "https";

    public static IResourceBuilder<TProject> WithCommonHealthChecks<TProject>(this IResourceBuilder<TProject> builder)
        where TProject : IResourceWithEndpoints
    {
        return builder
            .WithHttpHealthCheck("/health/startup")
            .WithHttpHealthCheck("/health/readiness")
            .WithHttpHealthCheck("/health/liveness");
    }

    public static IResourceBuilder<TProject> WithNonProxiedHttpsEndpoint<TProject>(this IResourceBuilder<TProject> builder)
        where TProject : IResourceWithEndpoints
    {
        return builder.WithEndpoint(_httpsEndpointName, endpoint => endpoint.IsProxied = false);
    }

    public static IResourceBuilder<TProject> WithSqliteMemory<TProject>(
        this IResourceBuilder<TProject> builder)
        where TProject : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment("ROME__DATABASE__DATABASE_TYPE", "Sqlite")
            .WithEnvironment("ROME__DATABASE__CONNECTION_STRING", "MEMORY");
    }
    public static IResourceBuilder<TProject> WithPostgresDatabase<TProject>(
        this IResourceBuilder<TProject> builder,
        IResourceBuilder<ParameterResource> username,
        IResourceBuilder<ParameterResource> password)
        where TProject : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment("ROME__DATABASE__DATABASE_TYPE", "Postgres")
            .WithEnvironment("ROME__DATABASE__DB_PORT", AspireConstants.PostgresPort.ToString())
            .WithEnvironment("ROME__DATABASE__DB_HOST", AspireConstants.DbHost)
            .WithEnvironment("ROME__DATABASE__DB_DATABASE", AspireConstants.PostgresDatabase)
            .WithEnvironment("ROME__DATABASE__DB_USERNAME", username)
            .WithEnvironment("ROME__DATABASE__DB_PASSWORD", password);
    }

    public static IResourceBuilder<TProject> WithRedis<TProject>(
        this IResourceBuilder<TProject> builder,
        IResourceBuilder<RedisResource> redis)
        where TProject : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment("ROME__REDIS__INSTANCE_NAME", "rome-redis")
            .WithEnvironment("ROME__REDIS__HOST", "localhost")
            .WithEnvironment("ROME__REDIS__PORT", redis.Resource.Host.Endpoint.TargetPort.ToString())
            .WithEnvironment("ROME__REDIS__PASSWORD", redis.Resource.PasswordParameter!)
            .WithReference(redis);
    }

    public static IResourceBuilder<TProject> WithConfig<TProject>(
        this IResourceBuilder<TProject> builder,
        IConfigurationRoot config)
        where TProject : IResourceWithEnvironment
    {
        foreach (var c in config.AsEnumerable())
        {
            if (!string.IsNullOrEmpty(c.Value))
            {
                builder.WithEnvironment(c.Key.Replace(":", "__"), c.Value);
            }
        }

        return builder;
    }
}
