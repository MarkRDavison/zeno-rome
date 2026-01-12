namespace mark.davison.rome.web.components.Pages.Account.Edit;

public class EditAccountViewModel : BaseViewModel<(Guid, EditAccountFormViewModel?)>
{
    private Guid _accountTypeId;
    private readonly IStartupState _startupState;
    private bool _isCreateNew;

    public EditAccountViewModel(IStartupState startupState)
    {
        _startupState = startupState;

        RegisterState(_startupState);
    }

    public override async Task<bool> Initialize((Guid, EditAccountFormViewModel?) payload)
    {
        await Task.CompletedTask;

        if (_accountTypeId != payload.Item1)
        {
            return false;
        }

        _accountTypeId = payload.Item1;

        // TODO: Factory/from DI?
        _isCreateNew = payload.Item2 is null;
        FormViewModel = payload.Item2 ?? new EditAccountFormViewModel(_startupState);

        return true;
    }

    public async Task OnCreate()
    {
        InProgress = true;
        await Task.CompletedTask;
        InProgress = false;
    }

    public string Title => _isCreateNew ? "Create account" : "Edit account";
    public bool Loading => FormViewModel is null;
    public bool InProgress { get; private set; }
    public bool PrimaryDisabled => InProgress;
    public EditContext? EditContext { get; private set; }
    public required EditAccountFormViewModel FormViewModel { get; set; }
}
