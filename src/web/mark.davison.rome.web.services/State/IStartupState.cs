namespace mark.davison.rome.web.services.State;

public interface IStartupState : IStateService
{

    bool BootstrapComplete { get; }
    IList<string> AuthProviders { get; }
    List<AccountTypeDto> AccountTypes { get; }
    List<CurrencyDto> Currencies { get; }
    List<TransactionTypeDto> TransactionTypes { get; }
    void NotifyStateChanged(IList<string> providers);
    void NotifyStateChanged(StartupDataDto startupData);
}
