namespace mark.davison.rome.web.components.tests.Forms.EditTransaction;

public sealed class EditTransactionFormTagsTests
{
    [Test]
    public async Task ClickingManageTagsLink_OpensTagManagerShell()
    {
        var shell = new Mock<ITagManagerShell>();
        var tagState = CreateTagStateMock();
        var formViewModel = CreateFormViewModel(tagState);

        using var ctx = new RomeComponentTestContext();
        var cut = RenderForm(ctx, formViewModel, shell.Object, tagState.Object);

        cut.Find($"[data-testid='{DataTestIds.ManageTagsLink}']").Click();

        shell.Verify(_ => _.Open(), Times.Once);
    }

    [Test]
    public async Task RendersTagMultiSelect_WhenTagsExist()
    {
        var tagId = Guid.NewGuid();
        var tagState = CreateTagStateMock([new TagDto(tagId, "Holiday")]);

        var formViewModel = CreateFormViewModel(tagState);
        formViewModel.AddSplit();

        using var ctx = new RomeComponentTestContext();
        var cut = RenderForm(ctx, formViewModel, new Mock<ITagManagerShell>().Object, tagState.Object);

        cut.Find($"[data-testid='{DataTestIds.TagMultiSelectPrefix}0']");
    }

    [Test]
    public async Task ShowsNoTagsMessage_WhenTagListEmpty()
    {
        var tagState = CreateTagStateMock();
        var formViewModel = CreateFormViewModel(tagState);
        formViewModel.AddSplit();

        using var ctx = new RomeComponentTestContext();
        var cut = RenderForm(ctx, formViewModel, new Mock<ITagManagerShell>().Object, tagState.Object);

        await Assert.That(cut.Markup).Contains("No tags yet.");
        await Assert.That(cut.Markup.Contains(DataTestIds.TagMultiSelectPrefix)).IsFalse();
    }

    private static Mock<ITagState> CreateTagStateMock(IList<TagDto>? tags = null)
    {
        var tagState = new Mock<ITagState>();
        tagState.Setup(_ => _.Tags).Returns(tags ?? []);
        tagState.Setup(_ => _.Loading).Returns(false);
        tagState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);
        return tagState;
    }

    private static EditTransactionFormViewModel CreateFormViewModel(Mock<ITagState> tagState)
    {
        var startupState = new Mock<IStartupState>();
        startupState.Setup(_ => _.AccountTypes).Returns([]);
        startupState.Setup(_ => _.Currencies).Returns([]);
        startupState.Setup(_ => _.TransactionTypes).Returns([]);

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.Accounts).Returns([]);

        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.Categories).Returns([]);

        return new EditTransactionFormViewModel(
            startupState.Object,
            accountState.Object,
            categoryState.Object,
            tagState.Object)
        {
            TransactionTypeId = TransactionTypeConstants.Withdrawal
        };
    }

    private static IRenderedComponent<EditTransactionForm> RenderForm(
        RomeComponentTestContext ctx,
        EditTransactionFormViewModel formViewModel,
        ITagManagerShell shell,
        ITagState tagState)
    {
        ctx.Services.AddSingleton(shell);
        ctx.Services.AddSingleton(tagState);

        return ctx.RenderComponentWithPopoverProvider<EditTransactionForm>(parameters => parameters
            .Add(p => p.FormViewModel, formViewModel)
            .Add(p => p.Loading, false)
            .Add(p => p.Processing, false));
    }
}
