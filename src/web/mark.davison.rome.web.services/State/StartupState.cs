namespace mark.davison.rome.web.services.State;

internal sealed class StartupState : IStartupState
{
    public bool BootstrapComplete { get; private set; }
    public IList<string> AuthProviders { get; private set; } = [];

    public List<AccountTypeDto> AccountTypes { get; private set; } = [];
    public List<CurrencyDto> Currencies { get; private set; } = [];
    public List<TransactionTypeDto> TransactionTypes { get; private set; } = [];
    public bool Loading { get; }
    public bool Loaded { get; }

    public event EventHandler StateChanged = default!;
    public void NotifyStateChanged(IList<string> providers)
    {
        AuthProviders = [.. providers];
        BootstrapComplete = true;

        NotifyStateChanged();
    }

    public void NotifyStateChanged(StartupDataDto startupData)
    {
        AccountTypes = [.. startupData.AccountTypes];
        Currencies = [.. startupData.Currencies];
        TransactionTypes = [.. startupData.TransactionTypes];

        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
