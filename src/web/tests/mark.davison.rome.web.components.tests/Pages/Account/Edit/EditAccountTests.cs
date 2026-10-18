namespace mark.davison.rome.web.components.tests.Pages.Account.Edit;

public sealed class EditAccountTests
{
    [Test]
    public async Task RendersLoadedAccountFields()
    {
        var accountId = Guid.NewGuid();
        var viewModel = BuildViewModel(accountId, "Savings jar");

        using var ctx = new RomeComponentTestContext();
        ctx.Services.AddSingleton(viewModel);

        var cut = ctx.RenderComponentWithPopoverProvider<EditAccount>(parameters => parameters.Add(p => p.Id, accountId));

        await Assert.That(cut.Find("#edit-account-form-name").GetAttribute("value")).Contains("Savings jar");
        await Assert.That(cut.Find("#edit-account-form-account-type-readonly").GetAttribute("value")).Contains("Asset");
    }

    private static EditAccountViewModel BuildViewModel(Guid accountId, string name)
    {
        var currencyId = CurrencyConstants.NZD;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.FetchAccount(accountId)).Returns(Task.CompletedTask);
        accountState.Setup(_ => _.Accounts).Returns([
            new AccountDto(
                accountId,
                name,
                AccountTypeConstants.Asset,
                "123",
                CurrencyRules.ToPersisted(1500m),
                true,
                DateTimeOffset.UtcNow,
                CurrencyRules.ToPersisted(1500m),
                currencyId,
                null,
                CurrencyRules.ToPersisted(1500m),
                today)
        ]);
        accountState.Setup(_ => _.Loading).Returns(false);
        accountState.Setup(_ => _.Loaded).Returns(true);

        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.Currencies).Returns([new CurrencyDto(currencyId, "NZD", "NZ Dollar", "$", 2)]);
        startupState.Setup(_ => _.AccountTypes).Returns([new AccountTypeDto(AccountTypeConstants.Asset, "Asset")]);
        startupState.Setup(_ => _.ReferenceDataLoaded).Returns(true);
        startupState.Setup(_ => _.Loading).Returns(false);
        startupState.Setup(_ => _.Loaded).Returns(true);

        var appContext = new Mock<IAppContextService>();
        var month = DateOnly.FromDateTime(DateTime.Today).GetMonthRange();
        appContext.Setup(_ => _.State).Returns(new AppContextState(month.Item1, month.Item2));

        var bootstrap = new Mock<IAppSessionBootstrap>();
        bootstrap.Setup(_ => _.EnsureAuthenticatedStartupAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var viewModel = new EditAccountViewModel(
            startupState.Object,
            accountState.Object,
            new Mock<IFormSubmission<EditAccountFormViewModel>>().Object,
            new Mock<IClientNavigationManager>().Object,
            appContext.Object,
            bootstrap.Object)
        {
            FormViewModel = new EditAccountFormViewModel(startupState.Object)
        };

        viewModel.InitializeForEdit(accountId).GetAwaiter().GetResult();
        return viewModel;
    }
}
