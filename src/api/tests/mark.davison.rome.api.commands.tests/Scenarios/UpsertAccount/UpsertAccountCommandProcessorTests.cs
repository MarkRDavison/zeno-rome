namespace mark.davison.rome.api.commands.tests.Scenarios.UpsertAccount;

public class UpsertAccountCommandProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IDateService> _dateService;
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock;
    private readonly UpsertAccountCommandProcessor _upsertAccountCommandProcessor;

    public UpsertAccountCommandProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);
        _dateService = new(MockBehavior.Strict);
        _commandDispatcherMock = new(MockBehavior.Strict);

        _upsertAccountCommandProcessor = new(_dateService.Object, _dbContext, _commandDispatcherMock.Object);

        _currentUserContextMock.Setup(_ => _.UserId).Returns(Guid.Empty);
        _dateService.Setup(_ => _.Now).Returns(DateTime.Now);

        _commandDispatcherMock
            .Setup(_ => _.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                It.IsAny<CreateTransactionCommandRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateTransactionCommandResponse());
    }

    [Test]
    public async Task WhenOpeningBalanceSpecified_InitialBalanceTransactionAreCreated()
    {
        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, string.Empty, null, string.Empty, Guid.Empty, CurrencyConstants.NZD, CurrencyRules.ToPersisted(100.0M), DateOnly.FromDateTime(DateTime.Today))
        };

        _commandDispatcherMock
            .Setup(_ => _.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                It.IsAny<CreateTransactionCommandRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (CreateTransactionCommandRequest r, CancellationToken c) =>
            {
                await Assert.That(r.TransactionTypeId).EqualTo(TransactionTypeConstants.OpeningBalance);
                var transaction = r.Transactions[0];
                await Assert.That(r.TransactionTypeId).IsNotEmptyGuid();
                await Assert.That(request.UpsertAccountDto.OpeningBalance).EqualTo(transaction.Amount);
                await Assert.That(request.UpsertAccountDto.OpeningBalanceDate).EqualTo(transaction.Date);
                await Assert.That(transaction.ForeignCurrencyId).IsNull();
                await Assert.That(transaction.ForeignAmount).IsNull();
                await Assert.That(transaction.Description).IsNotNullOrEmpty();
                await Assert.That(transaction.SourceAccountId).EqualTo(AccountConstants.OpeningBalance);
                await Assert.That(transaction.CurrencyId).EqualTo(request.UpsertAccountDto.CurrencyId);

                return new CreateTransactionCommandResponse();
            })
            .Verifiable();

        await _upsertAccountCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        var persistedAccount = await _dbContext.GetByIdAsync<Account>(request.UpsertAccountDto.Id, CancellationToken.None);

        await Assert.That(persistedAccount).IsNotNull();

        _commandDispatcherMock
            .Verify(_ =>
                _.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                    It.IsAny<CreateTransactionCommandRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Test]
    public async Task WhenOpeningBalanceNotSpecified_InitialBalanceTransactionNotCreated()
    {
        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, string.Empty, null, string.Empty, Guid.Empty, CurrencyConstants.NZD, null, null)
        };

        _commandDispatcherMock
            .Setup(_ => _.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                It.IsAny<CreateTransactionCommandRequest>(),
                It.IsAny<CancellationToken>()))
            .Verifiable();

        await _upsertAccountCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        _commandDispatcherMock
            .Verify(_ =>
                _.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                    It.IsAny<CreateTransactionCommandRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Test]
    public async Task WhenOpeningBalanceSpecified_ForExistingAccountWithNonExistantOpeningBalance_InitialBalanceTransactionAreCreated()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CurrencyId = CurrencyConstants.NZD,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(account.Id, string.Empty, null, string.Empty, Guid.Empty, CurrencyConstants.NZD, CurrencyRules.ToPersisted(100.0M), DateOnly.FromDateTime(DateTime.Today))
        };

        await _dbContext.AddAsync(account, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _commandDispatcherMock
            .Setup(_ => _.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                It.IsAny<CreateTransactionCommandRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (CreateTransactionCommandRequest r, CancellationToken c) => new CreateTransactionCommandResponse())
            .Verifiable();

        await _upsertAccountCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        _commandDispatcherMock
            .Verify(_ =>
                _.Dispatch<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                    It.IsAny<CreateTransactionCommandRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Test]
    public async Task ExistingAccount_ExistingOpeningBalance_RequestRemovesOpeningBalance()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CurrencyId = CurrencyConstants.NZD,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        transaction.TransactionJournal = new TransactionJournal
        {
            Id = Guid.NewGuid(),
            TransactionTypeId = TransactionTypeConstants.OpeningBalance,
            Transactions =
            [
                transaction,
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = AccountConstants.OpeningBalance,
                    UserId = Guid.Empty,
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                }
            ],
            TransactionGroup = new TransactionGroup
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            },
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(account.Id, string.Empty, null, string.Empty, Guid.Empty, CurrencyConstants.NZD, null, null)
        };

        await _dbContext.AddAsync(account, CancellationToken.None);
        await _dbContext.AddAsync(transaction, CancellationToken.None);
        await _dbContext.AddAsync(transaction.TransactionJournal, CancellationToken.None);
        await _dbContext.AddAsync(transaction.TransactionJournal.TransactionGroup, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        await _upsertAccountCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        var fetchedTransaction = await _dbContext.GetByIdAsync<Transaction>(transaction.Id, CancellationToken.None);
        var fetchedTransactionJournal = await _dbContext.GetByIdAsync<Transaction>(transaction.TransactionJournal.Id, CancellationToken.None);
        var fetchedTransactionGroup = await _dbContext.GetByIdAsync<Transaction>(transaction.TransactionJournal.TransactionGroup.Id, CancellationToken.None);

        await Assert.That(fetchedTransaction).IsNull();
        await Assert.That(fetchedTransactionJournal).IsNull();
        await Assert.That(fetchedTransactionGroup).IsNull();
    }

    [Test]
    public async Task ExistingAccount_ExistingOpeningBalance_RequestEditsOpeningBalance()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CurrencyId = CurrencyConstants.NZD,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        transaction.TransactionJournal = new TransactionJournal
        {
            Id = Guid.NewGuid(),
            TransactionTypeId = TransactionTypeConstants.OpeningBalance,
            Transactions =
            [
                transaction,
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = AccountConstants.OpeningBalance,
                    UserId = Guid.Empty,
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                }
            ],
            TransactionGroup = new TransactionGroup
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            },
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(account.Id, string.Empty, null, string.Empty, Guid.Empty, CurrencyConstants.NZD, CurrencyRules.ToPersisted(100.0M), DateOnly.FromDateTime(DateTime.Today))
        };

        await _dbContext.AddAsync(account, CancellationToken.None);
        await _dbContext.AddAsync(transaction, CancellationToken.None);
        await _dbContext.AddAsync(transaction.TransactionJournal, CancellationToken.None);
        await _dbContext.AddAsync(transaction.TransactionJournal.TransactionGroup, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        await _upsertAccountCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        {
            var persitedTransactions = await _dbContext
                .Set<Transaction>()
                .Where(_ => _.TransactionJournal!.Id == transaction.TransactionJournal.Id)
                .ToListAsync(CancellationToken.None);

            var sourceAccountTransaction = persitedTransactions.First(_ => _.AccountId == AccountConstants.OpeningBalance);
            var destinationAccountTransaction = persitedTransactions.First(_ => _.AccountId == account.Id);

            await Assert.That(sourceAccountTransaction.Amount).IsEqualTo(-request.UpsertAccountDto.OpeningBalance!.Value);
            await Assert.That(destinationAccountTransaction.Amount).IsEqualTo(+request.UpsertAccountDto.OpeningBalance!.Value);

            var persitedTransactionJournal = await _dbContext
                .Set<TransactionJournal>()
                .Where(_ => _.Id == transaction.TransactionJournal.Id)
                .FirstOrDefaultAsync(CancellationToken.None);

            await Assert.That(persitedTransactionJournal).IsNotNull();
            await Assert.That(persitedTransactionJournal.Date).IsEqualTo(request.UpsertAccountDto.OpeningBalanceDate!.Value);
        }
    }

    [Test]
    public async Task ExistingAccount_ExistingOpeningBalance_RequestDoesNotChangeOpeningBalance()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CurrencyId = CurrencyConstants.NZD,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = +CurrencyRules.ToPersisted(100.0M),
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        transaction.TransactionJournal = new TransactionJournal
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            TransactionTypeId = TransactionTypeConstants.OpeningBalance,
            Transactions =
            [
                transaction,
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = AccountConstants.OpeningBalance,
                    Amount = -CurrencyRules.ToPersisted(100.0M),
                    UserId = Guid.Empty,
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                }
            ],
            TransactionGroup = new TransactionGroup
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            },
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(account.Id, string.Empty, null, string.Empty, Guid.Empty, CurrencyConstants.NZD, CurrencyRules.ToPersisted(100.0M), transaction.TransactionJournal.Date)
        };

        await _dbContext.AddAsync(account, CancellationToken.None);
        await _dbContext.AddAsync(transaction, CancellationToken.None);
        await _dbContext.AddAsync(transaction.TransactionJournal, CancellationToken.None);
        await _dbContext.AddAsync(transaction.TransactionJournal.TransactionGroup, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        await _upsertAccountCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        {
            var persitedTransactions = await _dbContext
                .Set<Transaction>()
                .Where(_ => _.TransactionJournal!.Id == transaction.TransactionJournal.Id)
                .ToListAsync(CancellationToken.None);

            var sourceAccountTransaction = persitedTransactions.First(_ => _.AccountId == AccountConstants.OpeningBalance);
            var destinationAccountTransaction = persitedTransactions.First(_ => _.AccountId == account.Id);

            await Assert.That(sourceAccountTransaction.Amount).IsEqualTo(-request.UpsertAccountDto.OpeningBalance!.Value);
            await Assert.That(destinationAccountTransaction.Amount).IsEqualTo(+request.UpsertAccountDto.OpeningBalance!.Value);

            var persitedTransactionJournal = await _dbContext
                .Set<TransactionJournal>()
                .Where(_ => _.Id == transaction.TransactionJournal.Id)
                .FirstOrDefaultAsync(CancellationToken.None);

            await Assert.That(persitedTransactionJournal).IsNotNull();
            await Assert.That(persitedTransactionJournal.Date).IsEqualTo(request.UpsertAccountDto.OpeningBalanceDate!.Value);
        }
    }
}
