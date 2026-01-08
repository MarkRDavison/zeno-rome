namespace mark.davison.rome.web.components.Controls;

public partial class QuickAdd
{
    private class QuickAddItem
    {
        public required string Text { get; init; }
        public required string Href { get; init; }
        public required string Icon { get; init; }
        public required bool FlipX { get; init; }
    }

    private List<QuickAddItem> Items { get; init; }

    private void OnLinkClick(string href)
    {
        OnSelect();
        Navigation.NavigateTo(href);
    }

    [Parameter, EditorRequired]
    public required Action OnSelect { get; set; }

    [Inject]
    public required IClientNavigationManager Navigation { get; set; }

    public QuickAdd()
    {
        Items = [

            new()
            {
                Text = "New withdrawal",
                Href = RouteHelpers.TransactionNew(TransactionTypeConstants.Withdrawal),
                Icon = Icons.Material.Filled.ArrowRightAlt,
                FlipX = true
            },
            new()
            {
                Text = "New deposit",
                Href = RouteHelpers.TransactionNew(TransactionTypeConstants.Deposit),
                Icon = Icons.Material.Filled.ArrowRightAlt,
                FlipX = false
            },
            new()
            {
                Text = "New transfer",
                Href = RouteHelpers.TransactionNew(TransactionTypeConstants.Transfer),
                Icon = Icons.Material.Filled.SyncAlt,
                FlipX = false
            },
            new()
            {
                Text = "New asset account",
                Href = RouteHelpers.AccountNew(AccountTypeConstants.Asset),
                Icon = Icons.Material.Filled.QuestionMark,
                FlipX = false
            },
            new()
            {
                Text = "New expense account",
                Href = RouteHelpers.AccountNew(AccountTypeConstants.Expense),
                Icon = Icons.Material.Filled.QuestionMark,
                FlipX = false
            },
            new()
            {
                Text = "New revenue account",
                Href = RouteHelpers.AccountNew(AccountTypeConstants.Revenue),
                Icon = Icons.Material.Filled.QuestionMark,
                FlipX = false
            }
        ];
    }
}
