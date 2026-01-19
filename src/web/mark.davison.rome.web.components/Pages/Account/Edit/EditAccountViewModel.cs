namespace mark.davison.rome.web.components.Pages.Account.Edit;

public class EditAccountViewModel : BaseViewModel<(Guid, EditAccountFormViewModel?)>
{
    private Guid _accountTypeId;
    private readonly IStartupState _startupState;
    private readonly IFormSubmission<EditAccountFormViewModel> _formSubmission;
    private readonly IClientNavigationManager _clientNavigationManager;
    private bool _isCreateNew;

    public EditAccountViewModel(
        IStartupState startupState,
        IFormSubmission<EditAccountFormViewModel> formSubmission,
        IClientNavigationManager clientNavigationManager)
    {
        _startupState = startupState;
        _formSubmission = formSubmission;
        _clientNavigationManager = clientNavigationManager;

        RegisterState(_startupState);
    }

    // TODO: Handle null account type id, just have to select it
    public override async Task<bool> Initialize((Guid, EditAccountFormViewModel?) payload)
    {
        await Task.CompletedTask;

        if (_accountTypeId == payload.Item1)
        {
            return false;
        }

        _accountTypeId = payload.Item1;

        // TODO: Factory/from DI?
        _isCreateNew = payload.Item2 is null;
        FormViewModel = payload.Item2 ?? new EditAccountFormViewModel(_startupState)
        {
            AccountTypeId = _accountTypeId,
            HideAccountType = true
        };

        if (EditContext is not null)
        {
            EditContext.OnFieldChanged -= FieldChanged;
        }

        EditContext = new EditContext(FormViewModel);
        EditContext.OnFieldChanged -= FieldChanged;

        return true;
    }

    public async Task OnCreate()
    {
        InProgress = true;

        Console.WriteLine("EditAccountViewModel.OnCreate");
        if (FormViewModel.Valid)
        {
            Console.WriteLine("EditAccountViewModel.OnCreate.FormViewModel.Valid == true");
            var response = await _formSubmission.Primary(FormViewModel);
            if (response.Success)
            {
                Console.WriteLine("EditAccountViewModel.OnCreate.FormViewModel.Submit.Response is success"); ;
                _clientNavigationManager.NavigateTo(RouteHelpers.Account(FormViewModel.Id));
            }
        }

        InProgress = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (EditContext is not null)
            {
                EditContext.OnFieldChanged -= FieldChanged;
            }
        }
        base.Dispose(disposing);
    }

    public string Title => _isCreateNew ? "Create account" : "Edit account";
    public bool Loading => FormViewModel is null;
    public bool InProgress { get; private set; }
    public bool PrimaryDisabled => InProgress;
    public EditContext? EditContext { get; private set; }
    public required EditAccountFormViewModel FormViewModel { get; set; }
    private void FieldChanged(object? sender, FieldChangedEventArgs args) => this.State_StateChanged(this, EventArgs.Empty);
}
