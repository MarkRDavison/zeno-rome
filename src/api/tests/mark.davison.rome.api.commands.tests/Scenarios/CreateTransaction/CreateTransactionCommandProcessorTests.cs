namespace mark.davison.rome.api.commands.tests.Scenarios.CreateTransaction;

public class CreateTransactionCommandProcessorTests
{
    private readonly IDateService _dateService;
    private readonly Mock<ICurrentUserContext> _currentUserContext;
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly CreateTransactionCommandProcessor _processor;

    public CreateTransactionCommandProcessorTests()
    {
        _dateService = new DateService(DateService.DateMode.Utc);
        _currentUserContext = new(MockBehavior.Strict);
        _dbContext = DbContextHelpers.CreateInMemory<RomeDbContext>(_ => new RomeDbContext(_));

        _processor = new(_dateService, _dbContext);

        _currentUserContext.Setup(_ => _.UserId).Returns(Guid.Empty);
    }

    [Test]
    public async Task Process_CreatesTransactionGroup()
    {
        var request = new CreateTransactionCommandRequest
        {
            Description = "Split description",
            Transactions =
            {
                new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null),
                new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null)
            }
        };

        var response = await _processor.ProcessAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();

        var transactionGroupExists = await _dbContext
            .Set<TransactionGroup>()
            .Where(_ => _.Id == response.Group.Id && _.Title == request.Description)
            .AnyAsync(CancellationToken.None);

        await Assert.That(transactionGroupExists).IsTrue();
    }

    [Test]
    public async Task Process_CreatesTransactionGroup_WithoutDescription_IfOnlyOneTransaction()
    {
        var request = new CreateTransactionCommandRequest
        {
            Description = "Split description",
            Transactions =
            {
                new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null)
            }
        };

        var response = await _processor.ProcessAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();

        var transactionGroup = await _dbContext
            .Set<TransactionGroup>()
            .Where(_ => _.Id == response.Group.Id)
            .FirstOrDefaultAsync(CancellationToken.None);

        await Assert.That(transactionGroup).IsNotNull();
        await Assert.That(transactionGroup.Title).IsNullOrEmpty();
    }

    [Test]
    public async Task Process_CreatesTransactionJournals_WithExpectedValues()
    {
        var createTransactionDto = new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null)
        {
            Description = "Some transaction description",
            CurrencyId = Guid.NewGuid(),
            ForeignCurrencyId = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today)
        };
        var request = new CreateTransactionCommandRequest
        {
            TransactionTypeId = TransactionTypeConstants.Deposit,
            Transactions =
            {
                createTransactionDto,
                createTransactionDto
            }
        };

        var response = await _processor.ProcessAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();

        var tranasctionJournalIds = response.Journals.Select(_ => _.Id).ToList();

        var transactionJournals = await _dbContext
            .Set<TransactionJournal>()
            .Where(_ => tranasctionJournalIds.Contains(_.Id))
            .ToListAsync(CancellationToken.None);

        for (int i = 0; i < transactionJournals.Count; ++i)
        {
            await Assert.That(transactionJournals[i].Id).IsNotEmptyGuid();
            await Assert.That(transactionJournals[i].TransactionGroupId).IsNotEmptyGuid();
            await Assert.That(transactionJournals[i].TransactionTypeId).EqualTo(request.TransactionTypeId);
            await Assert.That(transactionJournals[i].Description).EqualTo(createTransactionDto.Description);
            await Assert.That(transactionJournals[i].CurrencyId).EqualTo(createTransactionDto.CurrencyId);
            await Assert.That(transactionJournals[i].ForeignCurrencyId).EqualTo(createTransactionDto.ForeignCurrencyId);
            await Assert.That(transactionJournals[i].Date).EqualTo(createTransactionDto.Date);
            await Assert.That(transactionJournals[i].Order).EqualTo(i);
            await Assert.That(transactionJournals[i].Completed).IsFalse();
        }
    }

    [Test]
    public async Task Process_CreatesTransactions_AsExpected()
    {
        var transaction = new CreateTransactionDto(Guid.NewGuid(), "transaction description", Guid.NewGuid(), Guid.NewGuid(), DateOnly.MinValue, 100, 125, Guid.NewGuid(), Guid.NewGuid());
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            {
                transaction,
                transaction
            }
        };

        var response = await _processor.ProcessAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();

        var tranasctionIds = response.Transactions.Select(_ => _.Id).ToList();

        var transactions = await _dbContext
            .Set<Transaction>()
            .Where(_ => tranasctionIds.Contains(_.Id))
            .ToListAsync(CancellationToken.None);

        for (int i = 0; i < request.Transactions.Count; ++i)
        {
            var index = i * 2;
            var sourceTransaction = transactions[index + 0];
            var destinationTransaction = transactions[index + 1];

            await Assert.That(sourceTransaction.Id).IsNotEmptyGuid();
            await Assert.That(sourceTransaction.TransactionJournalId).IsNotEmptyGuid();
            await Assert.That(sourceTransaction.AccountId).EqualTo(request.Transactions[i].SourceAccountId);
            await Assert.That(sourceTransaction.CurrencyId).EqualTo(request.Transactions[i].CurrencyId);
            await Assert.That(sourceTransaction.ForeignCurrencyId).EqualTo(request.Transactions[i].ForeignCurrencyId);
            await Assert.That(sourceTransaction.Description).EqualTo(request.Transactions[i].Description);
            await Assert.That(sourceTransaction.Amount).EqualTo(-request.Transactions[i].Amount);
            await Assert.That(sourceTransaction.ForeignAmount).EqualTo(-request.Transactions[i].ForeignAmount);
            await Assert.That(sourceTransaction.Reconciled).IsFalse();
            await Assert.That(sourceTransaction.IsSource).IsTrue();

            await Assert.That(destinationTransaction.Id).IsNotEmptyGuid();
            await Assert.That(destinationTransaction.TransactionJournalId).IsNotEmptyGuid();
            await Assert.That(destinationTransaction.AccountId).EqualTo(request.Transactions[i].DestinationAccountId);
            await Assert.That(destinationTransaction.CurrencyId).EqualTo(request.Transactions[i].CurrencyId);
            await Assert.That(destinationTransaction.ForeignCurrencyId).EqualTo(request.Transactions[i].ForeignCurrencyId);
            await Assert.That(destinationTransaction.Description).EqualTo(request.Transactions[i].Description);
            await Assert.That(destinationTransaction.Amount).EqualTo(request.Transactions[i].Amount);
            await Assert.That(destinationTransaction.ForeignAmount).EqualTo(request.Transactions[i].ForeignAmount);
            await Assert.That(destinationTransaction.Reconciled).IsFalse();
            await Assert.That(destinationTransaction.IsSource).IsFalse();
        }
    }

}
