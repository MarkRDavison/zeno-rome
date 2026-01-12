namespace mark.davison.rome.api.persistence;

internal sealed class RomeDataSeeder : IDataSeeder
{
    private readonly bool _isProductionMode;

    public RomeDataSeeder(
        bool isProductionMode
    )
    {
        _isProductionMode = isProductionMode;
    }

    public async Task SeedDataAsync(DbContext dbContext, CancellationToken token)
    {
        try
        {
            var user = await EnsureUserSeeded(dbContext, token);

            await EnsureTenantSeeded(dbContext, token);
            await EnsureRolesSeeded(dbContext, token);
            await EnsureAccountsSeeded(dbContext, token);
            await EnsureAccountTypesSeeded(dbContext, token);
            await EnsureCurrenciesSeeded(dbContext, token);
            await EnsureTransactionTypesSeeded(dbContext, token);

            await dbContext.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }

    static internal async Task EnsureSeeded<T>(DbContext dbContext, List<T> entities, CancellationToken cancellationToken)
        where T : RomeEntity
    {
        var existingEntities = await dbContext.Set<T>().Where(_ => _.UserId == Guid.Empty).ToListAsync(cancellationToken);

        var newEntities = entities.Where(_ => !existingEntities.Any(e => e.Id == _.Id)).ToList();

        await dbContext.Set<T>().AddRangeAsync(newEntities, cancellationToken);
    }

    private async Task EnsureDevAccountsSeeded(Guid userId, DbContext dbContext, CancellationToken cancellationToken)
    {
        var accounts = new List<Account> {
            new Account
            {
                Id = Guid.Parse("8E6DBBA7-CC8F-4D84-BAC6-FBD7889C8069"),
                UserId = userId,
                AccountTypeId = AccountTypeConstants.Asset,
                CurrencyId = CurrencyConstants.NZD,
                Name = "Everyday",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            },
            new Account
            {
                Id = Guid.Parse("6339A56A-D1F2-4304-8CE9-024CF665BECC"),
                UserId = userId,
                AccountTypeId = AccountTypeConstants.Asset,
                CurrencyId = CurrencyConstants.NZD,
                Name = "Savings",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            },
            new Account
            {
                Id = Guid.Parse("2C75EF75-AD77-4738-816E-B69C84F1BECF"),
                UserId = userId,
                AccountTypeId = AccountTypeConstants.Revenue,
                CurrencyId = CurrencyConstants.NZD,
                Name = "Work",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            },
            new Account
            {
                Id = Guid.Parse("5F15867D-CBF9-4F09-B957-ED2EBFA0B89D"),
                UserId = userId,
                AccountTypeId = AccountTypeConstants.Expense,
                CurrencyId = CurrencyConstants.NZD,
                Name = "Supermarket",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            },
            new Account
            {
                Id = Guid.Parse("62FC6CF0-E130-41C1-AE0C-3CF6DB2DA34D"),
                UserId = userId,
                AccountTypeId = AccountTypeConstants.Expense,
                CurrencyId = CurrencyConstants.NZD,
                Name = "Mechanic",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            }
        };

        await EnsureSeeded(dbContext, accounts, cancellationToken);
    }
    private async Task EnsureTransactionTypesSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        var transactionTypes = new List<TransactionType>
        {
            new TransactionType { Id = TransactionTypeConstants.Withdrawal, Type = "Withdrawal", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new TransactionType { Id = TransactionTypeConstants.Deposit, Type = "Deposit", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new TransactionType { Id = TransactionTypeConstants.Transfer, Type = "Transfer", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new TransactionType { Id = TransactionTypeConstants.OpeningBalance, Type = "Opening balance", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new TransactionType { Id = TransactionTypeConstants.Reconciliation, Type = "Reconciliation", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new TransactionType { Id = TransactionTypeConstants.Invalid, Type = "Invalid", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new TransactionType { Id = TransactionTypeConstants.LiabilityCredit, Type = "Liability credit", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue }
        };

        await EnsureSeeded(dbContext, transactionTypes, cancellationToken);
    }
    private async Task EnsureAccountTypesSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        var accountTypes = new List<AccountType>
        {
            new AccountType { Id = AccountTypeConstants.Default, Type = "Default", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Cash, Type = "Cash", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Asset, Type = "Asset", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Expense, Type = "Expense", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Revenue, Type = "Revenue", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.InitialBalance, Type = "Initial balance", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Beneficiary, Type = "Beneficiary", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Import, Type = "Import", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Loan, Type = "Loan", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Reconciliation, Type = "Reconcilation", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Debt, Type = "Debt", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.Mortgage, Type = "Mortgage", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new AccountType { Id = AccountTypeConstants.LiabilityCredit, Type = "Liability credit", UserId = Guid.Empty, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue }
        };

        await EnsureSeeded(dbContext, accountTypes, cancellationToken);
    }

    private async Task EnsureCurrenciesSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        var currencies = new List<Currency>
        {
            new Currency { Id = CurrencyConstants.NZD, UserId = Guid.Empty, Code = "NZD", Name = "New Zealand Dollar", Symbol = "NZ$", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.AUD, UserId = Guid.Empty, Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.USD, UserId = Guid.Empty, Code = "USD", Name = "US  Dollar", Symbol = "US$", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.CAD, UserId = Guid.Empty, Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.EUR, UserId = Guid.Empty, Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.GBP, UserId = Guid.Empty, Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.JPY, UserId = Guid.Empty, Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.RMB, UserId = Guid.Empty, Code = "RMB", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue},
            new Currency { Id = CurrencyConstants.INT, UserId = Guid.Empty, Code = "INT", Name = "Internal", Symbol = "$", DecimalPlaces = 2, Created = DateTimeOffset.MinValue, LastModified = DateTimeOffset.MinValue }
        };

        await EnsureSeeded(dbContext, currencies, cancellationToken);
    }

    private async Task EnsureAccountsSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        var accounts = new List<Account> {
            new Account
            {
                Id = AccountConstants.OpeningBalance,
                UserId = Guid.Empty,
                AccountTypeId = AccountTypeConstants.InitialBalance,
                CurrencyId = CurrencyConstants.INT,
                Name = "Opening balance",
                Created = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = AccountConstants.Reconciliation,
                UserId = Guid.Empty,
                AccountTypeId = AccountTypeConstants.Reconciliation,
                CurrencyId = CurrencyConstants.INT,
                Name = "Reconcilation",
                Created = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.MinValue
            }
        };

        await EnsureSeeded(dbContext, accounts, cancellationToken);
    }
    private async Task EnsureRolesSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync<Role>(dbContext, _ => _.Id == Guid.Parse("02a740de-569f-4477-b5e7-d8622228db17"), cancellationToken))
        {
            await dbContext.AddAsync(new Role
            {
                Id = Guid.Parse("02a740de-569f-4477-b5e7-d8622228db17"),
                Name = RoleConstants.Admin,
                Description = "Administrator with full access",
                Created = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.MinValue,
                UserId = Guid.Empty
            }, cancellationToken);
        }

        if (!await ExistsAsync<Role>(dbContext, _ => _.Id == Guid.Parse("207af3cb-4a21-4d85-a93d-e16a8690eff2"), cancellationToken))
        {
            // TODO: Constant ids?
            await dbContext.AddAsync(new Role
            {
                Id = Guid.Parse("207af3cb-4a21-4d85-a93d-e16a8690eff2"),
                Name = RoleConstants.User,
                Description = "Standard user with limited access",
                Created = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.MinValue,
                UserId = Guid.Empty
            }, cancellationToken);
        }
    }

    private async Task EnsureTenantSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await ExistsAsync<Tenant>(dbContext, _ => _.Id == TenantIds.SystemTenantId, cancellationToken))
        {
            await dbContext.AddAsync(new Tenant
            {
                Id = TenantIds.SystemTenantId,
                Name = "System",
                CreatedAt = DateTimeOffset.MinValue,
                LastModified = DateTimeOffset.MinValue
            }, cancellationToken);
        }
    }

    private async Task<User> EnsureUserSeeded(DbContext dbContext, CancellationToken cancellationToken)
    {
        var seededUser = new User
        {
            Id = Guid.Empty,
            TenantId = TenantIds.SystemTenantId,
            Email = "romesystem@markdavison.kiwi",
            DisplayName = "Rome System",
            CreatedAt = DateTimeOffset.MinValue,
            LastModified = DateTimeOffset.MinValue
        };

        var existingUser = await dbContext
            .Set<User>()
            .FindAsync(Guid.Empty, cancellationToken);

        if (existingUser == null)
        {
            await dbContext.AddAsync(seededUser, cancellationToken);
            existingUser = seededUser;
        }

        return existingUser;

    }

    private async Task<bool> ExistsAsync<TEntity>(
        DbContext dbContext,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken token)
        where TEntity : class
    {
        return await dbContext.Set<TEntity>().Where(predicate).AnyAsync(token);
    }

    public async Task SeedUserDataAsync(Guid userId, DbContext dbContext, CancellationToken token)
    {
        if (!_isProductionMode)
        {
            await EnsureDevAccountsSeeded(userId, dbContext, token);

            await dbContext.SaveChangesAsync(token);
        }
    }
}
