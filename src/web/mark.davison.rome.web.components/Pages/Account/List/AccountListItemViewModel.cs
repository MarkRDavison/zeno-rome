namespace mark.davison.rome.web.components.Pages.Account.List;

internal class AccountListItemViewModel
{
    public Guid Id { get; set; }
    public LinkDefinition? Name { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string CurrentBalance { get; set; } = string.Empty;
    public long CurrentBalanceAmount { get; set; }
    public bool Active { get; set; }
    public DateTime LastModified { get; set; }
    public string BalanceDifference { get; set; } = string.Empty;
    public long BalanceDifferenceAmount { get; set; }
}
