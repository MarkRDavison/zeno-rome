namespace mark.davison.rome.web.components.Pages.Account.View;

public class ViewAccountViewModel : BaseViewModel<Guid>
{
    private Guid _accountId;
    private readonly IAccountState _accountState;
    private readonly IStartupState _startupState;

    public ViewAccountViewModel(
        IAccountState accountState,
        IStartupState startupState)
    {
        _accountState = accountState;
        _startupState = startupState;

        RegisterState(_accountState);
        RegisterState(_startupState);
    }

    public override async Task<bool> Initialize(Guid payload)
    {
        _accountId = payload;

        await _accountState.FetchAccount(payload);

        return true;
    }

    private AccountDto? _account => _accountState.Accounts.FirstOrDefault(_ => _.Id == _accountId);
    public bool Loading => _accountId != Guid.Empty && _account is null;
    public string Title => _account?.Name ?? string.Empty;
}
