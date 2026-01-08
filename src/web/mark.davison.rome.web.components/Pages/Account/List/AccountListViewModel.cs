namespace mark.davison.rome.web.components.Pages.Account.List;

public class AccountListViewModel : INotifyPropertyChanged, IDisposable
{
    private bool _isLoading = true;
    private Guid? _accountTypeId;
    private readonly IStartupState _startupState;

    public AccountListViewModel(IStartupState startupState)
    {
        _startupState = startupState;

        _startupState.StateChanged += _startupState_StateChanged;
    }

    private void _startupState_StateChanged(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(null));
    }

    public async Task<bool> Initialize(Guid? accountTypeId)
    {
        if (_accountTypeId == accountTypeId)
        {
            return false;
        }

        await Task.CompletedTask;

        _accountTypeId = accountTypeId;
        _isLoading = false;

        return true;
    }

    public void Dispose()
    {
        _startupState.StateChanged -= _startupState_StateChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Loading =>
        _isLoading ||
        !_startupState.BootstrapComplete ||
        _startupState.AccountTypes.Count is 0;

    public string Title => Loading || _accountTypeId is null
        ? "Accounts"
        : $"{_startupState.AccountTypes.First(_ => _.Id == _accountTypeId).Type} accounts";
}
