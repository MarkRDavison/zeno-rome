using mark.davison.rome.web.components.Pages.Account.View;

namespace mark.davison.rome.web.components.tests.Pages.Account.View;

public sealed class ViewAccountTests
{
    [Test]
    public async Task RendersBalanceTitleAndRegisterRowsInOrder()
    {
        var accountId = Guid.NewGuid();
        var currencyId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var transactions = new List<TransactionDto>
        {
            CreateLeg(accountId, -1_000_000, true, "line a", groupId, journalId, 0, TransactionTypeConstants.Withdrawal, "Weekly shop", today, categoryId),
            CreateLeg(expenseId, 1_000_000, false, "line a", groupId, journalId, 0, TransactionTypeConstants.Withdrawal, "Weekly shop", today, null),
        };

        var viewModel = BuildViewModel(accountId, currencyId, expenseId, categoryId, transactions, 5_000_000);

        using var ctx = new RomeComponentTestContext();
        ctx.Services.AddSingleton(viewModel);

        var cut = ctx.RenderComponentWithPopoverProvider<ViewAccount>(parameters => parameters.Add(p => p.Id, accountId));

        await Assert.That(cut.Find($"[data-testid='{DataTestIds.ViewAccountTitle}']").TextContent).Contains("Checking");
        await Assert.That(cut.Find($"[data-testid='{DataTestIds.ViewAccountCurrentBalance}']").TextContent).Contains("$500.00");

        var register = cut.Find($"[data-testid='{DataTestIds.ViewAccountRegister}']");
        await Assert.That(register).IsNotNull();

        await Assert.That(cut.Find($"[data-testid='{DataTestIds.ViewAccountRegisterRowPrefix}0']").TextContent).Contains("Weekly shop");
        await Assert.That(cut.Find($"[data-testid='{DataTestIds.ViewAccountRegisterRowPrefix}1']").TextContent).Contains("line a");
        await Assert.That(cut.FindAll("td").Any(td => td.TextContent.Contains("Food"))).IsTrue();
    }

    private static ViewAccountViewModel BuildViewModel(
        Guid accountId,
        Guid currencyId,
        Guid expenseId,
        Guid categoryId,
        IList<TransactionDto> transactions,
        long currentBalance)
    {
        var transactionState = new Mock<ITransactionState>();
        transactionState.Setup(_ => _.Transactions).Returns(transactions);
        transactionState.Setup(_ => _.Loading).Returns(false);
        transactionState.Setup(_ => _.Loaded).Returns(true);
        transactionState.Setup(_ => _.FetchTransactionsForAccount(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.Accounts).Returns([
            new AccountDto(accountId, "Checking", AccountTypeConstants.Asset, string.Empty, currentBalance, true, false, DateTimeOffset.UtcNow, 0, currencyId, null, null, null),
            new AccountDto(expenseId, "Groceries", AccountTypeConstants.Expense, string.Empty, 0, true, false, DateTimeOffset.UtcNow, 0, currencyId, null, null, null)
        ]);
        accountState.Setup(_ => _.Loading).Returns(false);
        accountState.Setup(_ => _.Loaded).Returns(true);
        accountState.Setup(_ => _.FetchAccount(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.Currencies).Returns([new CurrencyDto(currencyId, "NZD", "NZ Dollar", "$", 2)]);
        startupState.Setup(_ => _.TransactionTypes).Returns([new TransactionTypeDto(TransactionTypeConstants.Withdrawal, "Withdrawal")]);
        startupState.Setup(_ => _.ReferenceDataLoaded).Returns(true);
        startupState.Setup(_ => _.Loading).Returns(false);
        startupState.Setup(_ => _.Loaded).Returns(true);

        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.Categories).Returns([new CategoryDto(categoryId, "Food")]);
        categoryState.Setup(_ => _.Loading).Returns(false);
        categoryState.Setup(_ => _.Loaded).Returns(true);
        categoryState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);

        var monthRange = DateOnly.FromDateTime(DateTime.Today).GetMonthRange();
        var appContextService = new Mock<IAppContextService>();
        appContextService.Setup(_ => _.State).Returns(new AppContextState(monthRange.Item1, monthRange.Item2));

        var navigation = new Mock<IClientNavigationManager>();
        var bootstrap = new Mock<IAppSessionBootstrap>();
        bootstrap.Setup(_ => _.EnsureAuthenticatedStartupAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var viewModel = new ViewAccountViewModel(
            accountState.Object,
            startupState.Object,
            transactionState.Object,
            categoryState.Object,
            appContextService.Object,
            navigation.Object,
            bootstrap.Object);

        viewModel.Initialize(accountId).GetAwaiter().GetResult();
        return viewModel;
    }

    private static TransactionDto CreateLeg(
        Guid accountId,
        long amount,
        bool source,
        string description,
        Guid groupId,
        Guid journalId,
        int journalOrder,
        Guid typeId,
        string splitTitle,
        DateOnly date,
        Guid? categoryId) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = amount,
            Source = source,
            Description = description,
            TransactionGroupId = groupId,
            TransactionJournalId = journalId,
            JournalOrder = journalOrder,
            CurrencyId = Guid.NewGuid(),
            Date = date,
            TransactionTypeId = typeId,
            SplitTransactionDescription = splitTitle,
            CategoryId = categoryId
        };
}
