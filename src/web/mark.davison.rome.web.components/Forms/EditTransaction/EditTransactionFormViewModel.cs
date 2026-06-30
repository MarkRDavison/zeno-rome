namespace mark.davison.rome.web.components.Forms.EditTransaction;

public sealed class EditTransactionFormViewModel : IFormViewModel
{
    private readonly IStartupState _startupState;
    private readonly IAccountState _accountState;
    private readonly ICategoryState _categoryState;

    public EditTransactionFormViewModel(
        IStartupState startupState,
        IAccountState accountState,
        ICategoryState categoryState)
    {
        _startupState = startupState;
        _accountState = accountState;
        _categoryState = categoryState;
    }

    public Guid Id { get; set; }
    public Guid TransactionTypeId { get; set; }
    public DateTime? Date { get; set; }
    public string SplitDescription { get; set; } = string.Empty;
    public bool Valid =>
        Items.All(_ => _.Valid) &&
        (Items.Count > 1
            ? (SplitValid)
            : (NonSplitValid));

    private bool SplitValid => !string.IsNullOrEmpty(SplitDescription);
    private bool NonSplitValid => string.IsNullOrEmpty(SplitDescription);

    public int GetDecimalPlacesForCurrencyId(Guid? currencyId) => _startupState.Currencies.FirstOrDefault(_ => _.Id == currencyId)?.DecimalPlaces ?? 2;

    public void AddSplit()
    {
        Items.Add(new EditTransactionFormViewModelItem
        {
            Id = Guid.NewGuid()
        });
    }

    public string GetSplitTitle(int index)
    {
        if (Items.Count <= 1)
        {
            return "Transaction information";
        }

        return $"Split {index + 1}/{Items.Count}";
    }

    public void RemoveSplit(Guid id)
    {
        var toRemove = Items.FirstOrDefault(_ => _.Id == id);

        if (toRemove != null)
        {
            Items.Remove(toRemove);
        }

        if (Items.Count == 1)
        {
            SplitDescription = string.Empty;
        }
    }

    public List<EditTransactionFormViewModelItem> Items { get; } = [];

    public IEnumerable<IDropdownItem> SourceAccountItems
    {
        get
        {
            var sourceAccountTypes = AllowableSourceDestinationAccounts.GetSourceAccountTypes(TransactionTypeId);

            var sourceAccountTypeNames = _startupState.AccountTypes
                .Where(_ => sourceAccountTypes.Contains(_.Id))
                .ToList();

            var acc = _accountState.Accounts
                .Where(_ => _.Active && sourceAccountTypes.Contains(_.AccountTypeId))
                .Select(_ => new DropdownItem
                {
                    Id = _.Id,
                    Name = _.Name
                });

            return acc;
        }
    }

    public IEnumerable<IDropdownItem> CategoryItems =>
        _categoryState.Categories.Select(_ => new DropdownItem
        {
            Id = _.Id,
            Name = _.Name
        });

    public IEnumerable<IDropdownItem> DestinationAccountItems
    {
        get
        {
            var destAccountTypes = AllowableSourceDestinationAccounts.GetDestinationAccountTypes(TransactionTypeId);

            return _accountState.Accounts
                .Where(_ => _.Active && destAccountTypes.Contains(_.AccountTypeId))
                .Select(_ => new DropdownItem
                {
                    Id = _.Id,
                    Name = _.Name
                });
        }
    }
    public IEnumerable<IDropdownItem> CurrencyItems => _startupState.Currencies.Select(_ => new DropdownItem { Id = _.Id, Name = _.Name });

}
