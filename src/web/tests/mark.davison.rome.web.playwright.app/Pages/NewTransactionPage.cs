namespace mark.davison.rome.web.playwright.app.Pages;

public sealed record NewTransactionGroupInfo(
    string? Description,
    List<NewTransactionSplitInfo> Splits);

public sealed record NewTransactionSplitInfo(
    string Name,
    string SourceAccountName,
    string DestinationAccountName,
    DateOnly Date,
    decimal Amount);

public sealed class NewTransactionPage : RomeBasePage
{
    private readonly TransactionType _transactionType;

    private const string AddSplitButtonLabel = "Add another split";
    private const string CreateTransactionButtonLabel = "Create transaction";
    private const string SplitTransactionDescriptionLabel = "Split transaction description";
    private const string SplitFieldNameLabel = "Name";
    private const string SplitFieldSourceAccountLabel = "Source account";
    private const string SplitFieldDestinationAccountLabel = "Destination account";
    private const string SplitFieldDateLabel = "Date";
    private const string SplitFieldAmountLabel = "Amount";
    private const string SplitFieldCategoryLabel = "Category";

    private static string SplitById(int index) => $"edit-transaction-form-{index}";

    public NewTransactionPage(
        IPage page,
        PlaywrightAppSettings appSettings,
        TransactionType transactionType
    ) : base(
        page,
        appSettings)
    {
        _transactionType = transactionType;
    }

    public async Task<ViewTransactionPage> Submit(NewTransactionGroupInfo info)
    {
        if (info.Splits.Count is > 0)
        {
            await FillSplit(0, info.Splits.First());

            if (info.Splits.Count is > 1)
            {
                for (int i = 1; i < info.Splits.Count; ++i)
                {
                    await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
                    {
                        Name = AddSplitButtonLabel
                    }).ClickAsync();
                    await FillSplit(i, info.Splits[i]);
                }

                await FillField(this, SplitTransactionDescriptionLabel, info.Description ?? string.Empty);
            }
        }

        return await Submit();
    }

    public async Task FillSplit(int index, NewTransactionSplitInfo splitInfo)
    {
        var splitLocator = Page.Locator($"id={SplitById(index)}");
        await splitLocator.IsVisibleAsync();

        await FillField(this, splitLocator, SplitFieldNameLabel, splitInfo.Name);
        await SelectField(this, splitLocator, SplitFieldSourceAccountLabel, splitInfo.SourceAccountName);
        await SelectField(this, splitLocator, SplitFieldDestinationAccountLabel, splitInfo.DestinationAccountName);
        if (index is 0)
        {
            // Date is only on the first split.
            await FillField(this, splitLocator, SplitFieldDateLabel, splitInfo.Date);
        }
        await FillField(this, splitLocator, SplitFieldAmountLabel, splitInfo.Amount);
        // TODO: CATEGORY
    }

    public async Task<ViewTransactionPage> Submit()
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = CreateTransactionButtonLabel
        }).ClickAsync();

        return await ViewTransactionPage.Goto(Page, AppSettings);
    }
}
