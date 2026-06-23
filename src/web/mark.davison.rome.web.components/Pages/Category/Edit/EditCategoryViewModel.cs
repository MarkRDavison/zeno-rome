namespace mark.davison.rome.web.components.Pages.Category.Edit;

public class EditCategoryViewModel : BaseViewModel<EditCategoryFormViewModel?>
{
    private readonly ICategoryState _categoryState;
    private readonly IClientNavigationManager _clientNavigationManager;
    private readonly IFormSubmission<EditCategoryFormViewModel> _formSubmission;
    private bool _isCreateNew;

    public EditCategoryViewModel(
        IAppContextService appContextService,
        ICategoryState categoryState,
        IClientNavigationManager clientNavigationManager,
        IFormSubmission<EditCategoryFormViewModel> formSubmission
    ) : base(
        appContextService)
    {
        _categoryState = categoryState;
        _clientNavigationManager = clientNavigationManager;
        _formSubmission = formSubmission;

        RegisterState(_categoryState);
    }

    public override async Task<bool> Initialize(EditCategoryFormViewModel? payload)
    {
        await Task.CompletedTask;

        _isCreateNew = payload is null;
        FormViewModel = payload ?? new EditCategoryFormViewModel(_categoryState);

        EditContext?.OnFieldChanged -= FieldChanged;
        EditContext = new EditContext(FormViewModel);
        EditContext.OnFieldChanged += FieldChanged;

        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            EditContext?.OnFieldChanged -= FieldChanged;
        }

        base.Dispose(disposing);
    }
    public async Task OnCreate()
    {
        InProgress = true;

        if (FormViewModel.Valid)
        {
            if (await _formSubmission.Primary(FormViewModel) is { Success: true })
            {
                _clientNavigationManager.NavigateTo(RouteHelpers.Category(FormViewModel.Id));
            }
        }

        InProgress = false;
    }

    public string Title => _isCreateNew ? "Create category" : "Edit category";
    public bool Loading => FormViewModel is null;
    public bool InProgress { get; private set; }
    public bool PrimaryDisabled => InProgress;
    public EditContext? EditContext { get; private set; }
    public required EditCategoryFormViewModel FormViewModel { get; set; }
    // TODO: BASE CLASS??
    private void FieldChanged(object? sender, FieldChangedEventArgs args) => this.State_StateChanged(this, EventArgs.Empty);

}
