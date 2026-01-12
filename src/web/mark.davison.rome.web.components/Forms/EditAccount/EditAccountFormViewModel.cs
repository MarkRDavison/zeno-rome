namespace mark.davison.rome.web.components.Forms.EditAccount;

public class EditAccountFormViewModel : IFormViewModel
{
    private readonly IStartupState _startupState;

    public EditAccountFormViewModel(IStartupState startupState)
    {
        _startupState = startupState;
    }

    public bool HideAccountType { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public Guid? AccountTypeId { get; set; }
    public Guid? CurrencyId { get; set; }
    public decimal? VirtualBalance { get; set; }
    public decimal? OpeningBalance { get; set; }
    public DateTime? OpeningBalanceDate { get; set; }

    public int? DecimalPlaces => _startupState.Currencies.FirstOrDefault(_ => _.Id == CurrencyId)?.DecimalPlaces;

    public IEnumerable<IDropdownItem> CurrencyItems => _startupState.Currencies.Select(_ => new DropdownItem { Id = _.Id, Name = _.Name });
    public IEnumerable<IDropdownItem> AccountTypes => _startupState.AccountTypes.Select(_ => new DropdownItem { Id = _.Id, Name = _.Type });

    public bool Valid =>
        !string.IsNullOrEmpty(Name) &&
        AccountTypeId != Guid.Empty && AccountTypeId != null &&
        CurrencyId != Guid.Empty && CurrencyId != null &&
        (OpeningBalance == default || OpeningBalanceDate != null);
}