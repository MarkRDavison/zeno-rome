using mark.davison.common.persistence;
using mark.davison.common.persistence.Helpers;
using mark.davison.common.persistence.migrations.postgres;
using mark.davison.rome.api.persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace mark.davison.rome.api.migrations.postgres;

[ExcludeFromCodeCoverage]
[DatabaseMigrationAssembly(DatabaseType.Postgres)]
public sealed class PostgresContextFactory : PostgresDbContextFactory<RomeDbContext>
{
    protected override string ConfigName => "DATABASE";

    protected override RomeDbContext DbContextCreation(
            DbContextOptions<RomeDbContext> options
        ) => new(options);

    public override DbConnectionInfo LoadDbConnectionInfo(IConfigurationSection config)
    {
        var info = base.LoadDbConnectionInfo(config);

        if (int.TryParse(config[nameof(DbConnectionInfo.PORT)], out var port))
        {
            info.PORT = port;
        }

        return info;
    }
}