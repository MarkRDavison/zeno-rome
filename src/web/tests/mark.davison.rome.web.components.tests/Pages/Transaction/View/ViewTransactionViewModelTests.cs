using mark.davison.rome.web.components.Pages.Transaction.View;
using mark.davison.rome.web.services.State.Transaction;

namespace mark.davison.rome.web.components.tests.Pages.Transaction.View;

public sealed class ViewTransactionViewModelTests
{
    [Test]
    public async Task Items_IncludesTagsFromSourceTransactionLine()
    {
        var transactionGroupId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var currencyId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var destAccountId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var sourceTransaction = new TransactionDto
        {
            Id = Guid.NewGuid(),
            TransactionGroupId = transactionGroupId,
            TransactionJournalId = journalId,
            TransactionTypeId = TransactionTypeConstants.Withdrawal,
            AccountId = sourceAccountId,
            CurrencyId = currencyId,
            Amount = -10000,
            Source = true,
            Description = "Groceries",
            Date = DateOnly.FromDateTime(DateTime.Today),
            Tags = [new TagDto(tagId, "Holiday")]
        };
        var destTransaction = new TransactionDto
        {
            Id = Guid.NewGuid(),
            TransactionGroupId = transactionGroupId,
            TransactionJournalId = journalId,
            TransactionTypeId = TransactionTypeConstants.Withdrawal,
            AccountId = destAccountId,
            CurrencyId = currencyId,
            Amount = 10000,
            Source = false,
            Description = "Groceries",
            Date = DateOnly.FromDateTime(DateTime.Today),
            Tags = [new TagDto(tagId, "Holiday")]
        };

        var transactionState = new Mock<ITransactionState>();
        transactionState.Setup(_ => _.FetchTransactions(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        transactionState.Setup(_ => _.Transactions).Returns([sourceTransaction, destTransaction]);
        transactionState.Setup(_ => _.Loading).Returns(false);
        transactionState.Setup(_ => _.Loaded).Returns(true);

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.FetchState(It.IsAny<Guid?>())).Returns(Task.CompletedTask);
        accountState.Setup(_ => _.Accounts).Returns([
            new AccountDto(sourceAccountId, "Checking", AccountTypeConstants.Asset, string.Empty, 0, true, false, DateTimeOffset.UtcNow, 0, currencyId, null, null, null),
            new AccountDto(destAccountId, "Supermarket", AccountTypeConstants.Expense, string.Empty, 0, true, false, DateTimeOffset.UtcNow, 0, currencyId, null, null, null)
        ]);
        accountState.Setup(_ => _.Loading).Returns(false);
        accountState.Setup(_ => _.Loaded).Returns(true);

        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.Currencies).Returns([new CurrencyDto(currencyId, "NZD", "New Zealand Dollar", "$", 2)]);
        startupState.Setup(_ => _.TransactionTypes).Returns([new TransactionTypeDto(TransactionTypeConstants.Withdrawal, "Withdrawal")]);
        startupState.Setup(_ => _.ReferenceDataLoaded).Returns(true);
        startupState.Setup(_ => _.Loading).Returns(false);
        startupState.Setup(_ => _.Loaded).Returns(true);

        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.Categories).Returns([]);
        categoryState.Setup(_ => _.Loading).Returns(false);
        categoryState.Setup(_ => _.Loaded).Returns(true);
        categoryState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);

        accountState.Setup(_ => _.FetchState(It.IsAny<Guid?>())).Returns(Task.CompletedTask);

        var bootstrap = new Mock<IAppSessionBootstrap>();
        bootstrap.Setup(_ => _.EnsureAuthenticatedStartupAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var viewModel = new ViewTransactionViewModel(
            accountState.Object,
            startupState.Object,
            categoryState.Object,
            transactionState.Object,
            new Mock<IAppContextService>().Object,
            bootstrap.Object,
            new Mock<IClientNavigationManager>().Object);

        await viewModel.Initialize(transactionGroupId);

        var items = viewModel.Items.ToList();
        await Assert.That(items).Count().IsEqualTo(1);
        await Assert.That(items[0].Tags).Count().IsEqualTo(1);
        await Assert.That(items[0].Tags[0].Name).IsEqualTo("Holiday");
        await Assert.That(items[0].Tags[0].Id).IsEqualTo(tagId);
    }
}
