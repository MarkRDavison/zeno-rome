namespace mark.davison.rome.web.components.Pages.Transaction.Edit;

public partial class EditTransactionViewModel : BaseViewModel<(Guid, EditTransactionFormViewModel?)>
{
    private Guid _transactionTypeId;
    private bool _isCreateNew;

    private readonly IStartupState _startupState;
    private readonly IAccountState _accountState;
    private readonly IFormSubmission<EditTransactionFormViewModel> _formSubmission;
    private readonly IClientNavigationManager _clientNavigationManager;

    public EditTransactionViewModel(
        IStartupState startupState,
        IAccountState accountState,
        IFormSubmission<EditTransactionFormViewModel> formSubmission,
        IClientNavigationManager clientNavigationManager)
    {
        _startupState = startupState;
        _accountState = accountState;

        RegisterState(_startupState);
        RegisterState(_accountState);
        _formSubmission = formSubmission;
        _clientNavigationManager = clientNavigationManager;
    }

    public override async Task<bool> Initialize((Guid, EditTransactionFormViewModel?) payload)
    {
        if (_transactionTypeId == payload.Item1)
        {
            return false;
        }

        _transactionTypeId = payload.Item1;
        _isCreateNew = payload.Item2 is null;

        FormViewModel = payload.Item2 ?? new EditTransactionFormViewModel(
            _startupState,
            _accountState)
        {
            TransactionTypeId = _transactionTypeId
        };

        EditContext?.OnFieldChanged -= FieldChanged;
        EditContext = new EditContext(FormViewModel);
        EditContext.OnFieldChanged += FieldChanged;

        await _accountState.FetchState(null);

        FormViewModel.AddSplit();

        return true;
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
    public async Task OnCreate()
    {
        InProgress = true;

        Console.WriteLine("EditTransactionViewModel.OnCreate");
        if (FormViewModel.Valid)
        {
            Console.WriteLine("EditTransactionViewModel.OnCreate.FormViewModel.Valid == true");
            if (await _formSubmission.Primary(FormViewModel) is { Success: true })
            {
                Console.WriteLine("EditTransactionViewModel.OnCreate.FormViewModel.Submit.Response is success"); ;
                _clientNavigationManager.NavigateTo(RouteHelpers.Transaction(FormViewModel.Id));
            }
        }

        InProgress = false;
    }

    public string Title => _isCreateNew ? "Create transaction" : "Edit transaction";
    public bool Loading => FormViewModel is null;
    public bool InProgress { get; private set; }
    public bool PrimaryDisabled => InProgress;

    public EditContext? EditContext { get; private set; }
    public required EditTransactionFormViewModel FormViewModel { get; set; }

    private void FieldChanged(object? sender, FieldChangedEventArgs args) => this.State_StateChanged(this, EventArgs.Empty);
}
