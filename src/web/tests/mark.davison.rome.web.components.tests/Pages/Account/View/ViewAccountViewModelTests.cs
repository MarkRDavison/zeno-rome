using mark.davison.rome.web.components.Pages.Account.View;

namespace mark.davison.rome.web.components.tests.Pages.Account.View;

public sealed class ViewAccountViewModelTests
{
    private readonly Guid _currencyId = Guid.NewGuid();
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Today);

    [Test]
    public async Task GenerateRows_ExcludesTransactionsOutsideDateRange()
    {
        var accountId = Guid.NewGuid();
        var inRange = CreatePair(accountId, Guid.NewGuid(), _today, groupId: Guid.NewGuid(), journalId: Guid.NewGuid(), journalOrder: 0);
        var outOfRange = CreatePair(accountId, Guid.NewGuid(), _today.AddMonths(-2), groupId: Guid.NewGuid(), journalId: Guid.NewGuid(), journalOrder: 0);

        var viewModel = CreateViewModel(
            accountId,
            [inRange.source, inRange.dest, outOfRange.source, outOfRange.dest],
            new AppContextState(_today.AddDays(-7), _today.AddDays(7)));

        var rows = viewModel.GenerateRows().ToList();

        await Assert.That(rows.Count(_ => !_.IsSplit)).IsEqualTo(1);
    }

    [Test]
    public async Task GenerateRows_IncludesTransactionsOnRangeBoundaries()
    {
        var accountId = Guid.NewGuid();
        var rangeStart = _today;
        var rangeEnd = _today.AddDays(7);
        var onStart = CreatePair(accountId, Guid.NewGuid(), rangeStart, Guid.NewGuid(), Guid.NewGuid(), 0);
        var onEnd = CreatePair(accountId, Guid.NewGuid(), rangeEnd, Guid.NewGuid(), Guid.NewGuid(), 0);

        var viewModel = CreateViewModel(
            accountId,
            [onStart.source, onStart.dest, onEnd.source, onEnd.dest],
            new AppContextState(rangeStart, rangeEnd));

        var rows = viewModel.GenerateRows().Where(_ => !_.IsSplit).ToList();

        await Assert.That(rows).Count().IsEqualTo(2);
    }

    [Test]
    public async Task GenerateRows_MultiSplit_OrdersHeaderThenJournalsAndSumsThisAccountLegs()
    {
        var assetId = Guid.NewGuid();
        var expense1 = Guid.NewGuid();
        var expense2 = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var journal1 = Guid.NewGuid();
        var journal2 = Guid.NewGuid();

        var transactions = new List<TransactionDto>
        {
            Leg(assetId, expense1, groupId, journal1, 0, "line a", -1_000_000, TransactionTypeConstants.Withdrawal, "Weekly shop"),
            Leg(expense1, assetId, groupId, journal1, 0, "line a", 1_000_000, TransactionTypeConstants.Withdrawal, "Weekly shop"),
            Leg(assetId, expense2, groupId, journal2, 1, "line b", -500_000, TransactionTypeConstants.Withdrawal, "Weekly shop"),
            Leg(expense2, assetId, groupId, journal2, 1, "line b", 500_000, TransactionTypeConstants.Withdrawal, "Weekly shop"),
        };

        var viewModel = CreateViewModel(assetId, transactions, MonthRange(_today));

        var rows = viewModel.GenerateRows().ToList();

        await Assert.That(rows[0].IsSplit).IsTrue();
        await Assert.That(rows[0].Description.Text).IsEqualTo("Weekly shop");
        await Assert.That(rows[0].Amount).IsEqualTo(-150m);
        await Assert.That(rows[1].Description.Text).IsEqualTo("line a");
        await Assert.That(rows[2].Description.Text).IsEqualTo("line b");
    }

    [Test]
    public async Task GenerateRows_Transfer_ShowsTransferTypeAndSignedAmount()
    {
        var assetA = Guid.NewGuid();
        var assetB = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var journalId = Guid.NewGuid();

        var transactions = new List<TransactionDto>
        {
            Leg(assetA, assetB, groupId, journalId, 0, "move", -2_500_000, TransactionTypeConstants.Transfer, string.Empty),
            Leg(assetB, assetA, groupId, journalId, 0, "move", 2_500_000, TransactionTypeConstants.Transfer, string.Empty),
        };

        var viewModel = CreateViewModel(assetA, transactions, MonthRange(_today));

        var row = viewModel.GenerateRows().Single(_ => !_.IsSplit);

        await Assert.That(row.TransactionType).IsEqualTo("Transfer");
        await Assert.That(row.TransactionTypeId).IsEqualTo(TransactionTypeConstants.Transfer);
        await Assert.That(row.Amount).IsEqualTo(-250m);
    }

    [Test]
    public async Task GenerateRows_CategoryNameAndHref_WhenCategorySet()
    {
        var assetId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var journalId = Guid.NewGuid();

        var source = Leg(assetId, expenseId, groupId, journalId, 0, "groceries", -1000, TransactionTypeConstants.Withdrawal, string.Empty);
        source.CategoryId = categoryId;
        var dest = Leg(expenseId, assetId, groupId, journalId, 0, "groceries", 1000, TransactionTypeConstants.Withdrawal, string.Empty);

        var viewModel = CreateViewModel(
            assetId,
            [source, dest],
            MonthRange(_today),
            [new CategoryDto(categoryId, "Food")]);

        var row = viewModel.GenerateRows().Single(_ => !_.IsSplit);

        await Assert.That(row.Category.Text).IsEqualTo("Food");
        await Assert.That(row.Category.Href).Contains(categoryId.ToString());
    }

    [Test]
    public async Task GenerateRows_SkipsJournalsNotInvolvingViewedAccount()
    {
        var assetId = Guid.NewGuid();
        var otherAsset = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var relevant = CreatePair(assetId, expenseId, _today, Guid.NewGuid(), Guid.NewGuid(), 0);
        var other = CreatePair(otherAsset, expenseId, _today, Guid.NewGuid(), Guid.NewGuid(), 0);

        var viewModel = CreateViewModel(
            assetId,
            [relevant.source, relevant.dest, other.source, other.dest],
            MonthRange(_today));

        var lineRows = viewModel.GenerateRows().Where(_ => !_.IsSplit).ToList();

        await Assert.That(lineRows).Count().IsEqualTo(1);
    }

    private ViewAccountViewModel CreateViewModel(
        Guid accountId,
        IList<TransactionDto> transactions,
        AppContextState appContext,
        IList<CategoryDto>? categories = null)
    {
        var transactionState = new Mock<ITransactionState>();
        transactionState.Setup(_ => _.Transactions).Returns(transactions);
        transactionState.Setup(_ => _.Loading).Returns(false);
        transactionState.Setup(_ => _.Loaded).Returns(true);
        transactionState.Setup(_ => _.FetchTransactionsForAccount(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.Accounts).Returns([
            new AccountDto(accountId, "Checking", AccountTypeConstants.Asset, string.Empty, 0, true, DateTimeOffset.UtcNow, 0, _currencyId, null, null, null),
            .. transactions.Select(t => t.AccountId).Distinct()
                .Where(id => id != accountId)
                .Select(id => new AccountDto(id, $"Acct-{id:N}", AccountTypeConstants.Expense, string.Empty, 0, true, DateTimeOffset.UtcNow, 0, _currencyId, null, null, null))
        ]);
        accountState.Setup(_ => _.Loading).Returns(false);
        accountState.Setup(_ => _.Loaded).Returns(true);
        accountState.Setup(_ => _.FetchAccount(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.Currencies).Returns([new CurrencyDto(_currencyId, "NZD", "NZ Dollar", "$", 2)]);
        startupState.Setup(_ => _.TransactionTypes).Returns([
            new TransactionTypeDto(TransactionTypeConstants.Withdrawal, "Withdrawal"),
            new TransactionTypeDto(TransactionTypeConstants.Transfer, "Transfer"),
            new TransactionTypeDto(TransactionTypeConstants.Deposit, "Deposit"),
        ]);
        startupState.Setup(_ => _.Loading).Returns(false);
        startupState.Setup(_ => _.Loaded).Returns(true);

        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.Categories).Returns(categories ?? []);
        categoryState.Setup(_ => _.Loading).Returns(false);
        categoryState.Setup(_ => _.Loaded).Returns(true);
        categoryState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);

        var appContextService = new Mock<IAppContextService>();
        appContextService.Setup(_ => _.State).Returns(appContext);

        var viewModel = new ViewAccountViewModel(
            accountState.Object,
            startupState.Object,
            transactionState.Object,
            categoryState.Object,
            appContextService.Object);

        viewModel.Initialize(accountId).GetAwaiter().GetResult();

        return viewModel;
    }

    private static AppContextState MonthRange(DateOnly date)
    {
        var (start, end) = date.GetMonthRange();
        return new AppContextState(start, end);
    }

    private static (TransactionDto source, TransactionDto dest) CreatePair(
        Guid sourceAccountId,
        Guid destAccountId,
        DateOnly date,
        Guid groupId,
        Guid journalId,
        int journalOrder,
        string splitTitle = "")
    {
        var source = Leg(sourceAccountId, destAccountId, groupId, journalId, journalOrder, "desc", -100, TransactionTypeConstants.Withdrawal, splitTitle);
        var dest = Leg(destAccountId, sourceAccountId, groupId, journalId, journalOrder, "desc", 100, TransactionTypeConstants.Withdrawal, splitTitle);
        source.Date = date;
        dest.Date = date;
        return (source, dest);
    }

    private static TransactionDto Leg(
        Guid accountId,
        Guid otherAccountId,
        Guid groupId,
        Guid journalId,
        int journalOrder,
        string description,
        long amount,
        Guid typeId,
        string splitTitle) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            TransactionGroupId = groupId,
            TransactionJournalId = journalId,
            JournalOrder = journalOrder,
            CurrencyId = Guid.NewGuid(),
            Amount = amount,
            Source = amount < 0,
            Description = description,
            Date = DateOnly.FromDateTime(DateTime.Today),
            TransactionTypeId = typeId,
            SplitTransactionDescription = splitTitle
        };
}
