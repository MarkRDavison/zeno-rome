using mark.davison.rome.web.components.Pages.Account.List;

namespace mark.davison.rome.web.components.tests.Pages.Account.List;

public sealed class AccountListViewModelTests
{
    [Test]
    public async Task OnAppContextUpdated_RefetchesAccountsWhenInitialized()
    {
        var appContext = new Mock<IAppContextService>();
        var month = DateOnly.FromDateTime(DateTime.Today).GetMonthRange();
        appContext.Setup(_ => _.State).Returns(new AppContextState(month.Item1, month.Item2));

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.Loading).Returns(false);
        accountState.Setup(_ => _.Accounts).Returns([]);
        accountState
            .Setup(_ => _.FetchState(It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);

        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.BootstrapComplete).Returns(true);
        startupState.Setup(_ => _.AccountTypes).Returns([new AccountTypeDto(Guid.NewGuid(), "Asset")]);

        var viewModel = new AccountListViewModel(
            startupState.Object,
            accountState.Object,
            new Mock<IClientNavigationManager>().Object,
            appContext.Object);

        await viewModel.Initialize(null);

        appContext.Raise(_ => _.RangeUpdated += null, EventArgs.Empty);

        await Task.Delay(50);

        accountState.Verify(_ => _.FetchState(null), Times.AtLeast(2));
    }
}
