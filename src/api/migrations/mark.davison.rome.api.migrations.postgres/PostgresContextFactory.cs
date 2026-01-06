using mark.davison.common.persistence;
using mark.davison.common.persistence.Helpers;
using mark.davison.common.persistence.migrations.postgres;
using mark.davison.rome.api.persistence;
using Microsoft.EntityFrameworkCore;
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
}