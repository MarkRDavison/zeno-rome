using mark.davison.common.persistence;
using mark.davison.common.persistence.Helpers;
using mark.davison.common.persistence.migrations.sqlite;
using mark.davison.rome.api.persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace mark.davison.rome.api.migrations.sqlite;

[ExcludeFromCodeCoverage]
[DatabaseMigrationAssembly(DatabaseType.Sqlite)]
public sealed class SqliteContextFactory : SqliteDbContextFactory<RomeDbContext>
{
    protected override RomeDbContext DbContextCreation(
            DbContextOptions<RomeDbContext> options
        ) => new(options);
}