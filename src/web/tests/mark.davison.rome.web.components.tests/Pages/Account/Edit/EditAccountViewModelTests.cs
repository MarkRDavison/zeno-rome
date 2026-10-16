namespace mark.davison.rome.web.components.tests.Pages.Account.Edit;

public sealed class EditAccountViewModelTests
{
    [Test]
    public async Task InitializeForEdit_LoadsFormFromAccountState()
    {
        var accountId = Guid.NewGuid();
        var currencyId = CurrencyConstants.NZD;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.FetchAccount(accountId)).Returns(Task.CompletedTask);
        accountState.Setup(_ => _.Accounts).Returns([
            new AccountDto(
                accountId,
                "Checking",
                AccountTypeConstants.Asset,
                "ACC-1",
                CurrencyRules.ToPersisted(2500m),
                true,
                DateTimeOffset.UtcNow,
                CurrencyRules.ToPersisted(2500m),
                currencyId,
                null,
                CurrencyRules.ToPersisted(2500m),
                today)
        ]);
        accountState.Setup(_ => _.Loading).Returns(false);
        accountState.Setup(_ => _.Loaded).Returns(true);

        var viewModel = CreateViewModel(accountState);

        await viewModel.InitializeForEdit(accountId);

        await Assert.That(viewModel.FormViewModel).IsNotNull();
        await Assert.That(viewModel.FormViewModel!.Id).IsEqualTo(accountId);
        await Assert.That(viewModel.FormViewModel.Name).IsEqualTo("Checking");
        await Assert.That(viewModel.FormViewModel.AccountNumber).IsEqualTo("ACC-1");
        await Assert.That(viewModel.FormViewModel.CurrencyId).IsEqualTo(currencyId);
        await Assert.That(viewModel.FormViewModel.OpeningBalance).IsEqualTo(2500m);
        await Assert.That(viewModel.FormViewModel.HideAccountType).IsTrue();
        await Assert.That(viewModel.Title).IsEqualTo("Edit account");
    }

    [Test]
    public async Task InitializeForEdit_UnknownAccount_NavigatesToNotFound()
    {
        var accountId = Guid.NewGuid();
        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.FetchAccount(accountId)).Returns(Task.CompletedTask);
        accountState.Setup(_ => _.Accounts).Returns([]);

        var navigation = new Mock<IClientNavigationManager>();
        var viewModel = CreateViewModel(accountState, navigation: navigation);

        await viewModel.InitializeForEdit(accountId);

        navigation.Verify(_ => _.NavigateTo(Routes.NotFound), Times.Once);
    }

    [Test]
    public async Task OnSave_Success_NavigatesToAccountView()
    {
        var accountId = Guid.NewGuid();
        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.FetchAccount(accountId)).Returns(Task.CompletedTask);
        accountState.Setup(_ => _.Accounts).Returns([
            new AccountDto(accountId, "Old", AccountTypeConstants.Asset, string.Empty, 0, true, DateTimeOffset.UtcNow, 0, CurrencyConstants.NZD, null, null, null)
        ]);
        accountState.Setup(_ => _.Loading).Returns(false);
        accountState.Setup(_ => _.Loaded).Returns(true);

        var submission = new Mock<IFormSubmission<EditAccountFormViewModel>>();
        submission
            .Setup(_ => _.Primary(It.IsAny<EditAccountFormViewModel>()))
            .ReturnsAsync(new UpsertAccountCommandResponse
            {
                Value = new AccountDto(accountId, "Updated", AccountTypeConstants.Asset, string.Empty, 0, true, DateTimeOffset.UtcNow, 0, CurrencyConstants.NZD, null, null, null)
            });

        var navigation = new Mock<IClientNavigationManager>();
        var viewModel = CreateViewModel(accountState, submission, navigation);

        await viewModel.InitializeForEdit(accountId);
        viewModel.FormViewModel!.Name = "Updated";

        await viewModel.OnSave();

        navigation.Verify(_ => _.NavigateTo(RouteHelpers.Account(accountId)), Times.Once);
    }

    private static EditAccountViewModel CreateViewModel(
        Mock<IAccountState>? accountState = null,
        Mock<IFormSubmission<EditAccountFormViewModel>>? submission = null,
        Mock<IClientNavigationManager>? navigation = null)
    {
        accountState ??= new Mock<IAccountState>();
        submission ??= new Mock<IFormSubmission<EditAccountFormViewModel>>();
        navigation ??= new Mock<IClientNavigationManager>();

        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.Currencies).Returns([new CurrencyDto(CurrencyConstants.NZD, "NZD", "NZ Dollar", "$", 2)]);
        startupState.Setup(_ => _.AccountTypes).Returns([new AccountTypeDto(AccountTypeConstants.Asset, "Asset")]);
        startupState.Setup(_ => _.Loading).Returns(false);
        startupState.Setup(_ => _.Loaded).Returns(true);

        var appContext = new Mock<IAppContextService>();
        var month = DateOnly.FromDateTime(DateTime.Today).GetMonthRange();
        appContext.Setup(_ => _.State).Returns(new AppContextState(month.Item1, month.Item2));

        return new EditAccountViewModel(
            startupState.Object,
            accountState.Object,
            submission.Object,
            navigation.Object,
            appContext.Object)
        {
            FormViewModel = new EditAccountFormViewModel(startupState.Object)
        };
    }
}
