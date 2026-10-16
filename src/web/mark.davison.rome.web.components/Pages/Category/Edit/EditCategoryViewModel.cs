namespace mark.davison.rome.web.components.Pages.Category.Edit;

public class EditCategoryViewModel : BaseViewModel<Guid?>
{
    private readonly ICategoryState _categoryState;
    private readonly IClientNavigationManager _clientNavigationManager;
    private readonly IFormSubmission<EditCategoryFormViewModel> _formSubmission;
    private Guid? _loadedCategoryId;
    private bool _initializing;
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

    public override async Task<bool> Initialize(Guid? categoryId)
    {
        if (categoryId == _loadedCategoryId && FormViewModel is not null)
        {
            return false;
        }

        _loadedCategoryId = categoryId;
        _isCreateNew = categoryId is null;
        _initializing = true;
        FormViewModel = null;
        NotifyPropertyChanged(nameof(Loading));

        if (!_isCreateNew)
        {
            await _categoryState.FetchState();

            var category = _categoryState.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category is null)
            {
                _initializing = false;
                _clientNavigationManager.NavigateTo(Routes.NotFound);
                return true;
            }

            FormViewModel = new EditCategoryFormViewModel(_categoryState)
            {
                Id = category.Id,
                Name = category.Name
            };
        }
        else
        {
            FormViewModel = new EditCategoryFormViewModel(_categoryState);
        }

        EditContext?.OnFieldChanged -= FieldChanged;
        EditContext = new EditContext(FormViewModel);
        EditContext.OnFieldChanged += FieldChanged;

        _initializing = false;
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

    public async Task OnSave()
    {
        if (FormViewModel is null)
        {
            return;
        }

        InProgress = true;

        if (FormViewModel.Valid)
        {
            if (await _formSubmission.Primary(FormViewModel) is { Success: true })
            {
                await _categoryState.FetchState();

                if (_isCreateNew)
                {
                    _clientNavigationManager.NavigateTo(RouteHelpers.Category(FormViewModel.Id));
                }
                else
                {
                    _clientNavigationManager.NavigateTo(Routes.Categories);
                }
            }
        }

        InProgress = false;
    }

    public string Title => _isCreateNew ? "Create category" : "Edit category";
    public string PrimaryButtonText => _isCreateNew ? "Create category" : "Save category";
    public bool Loading => _initializing || IsStateLoading || FormViewModel is null;
    public bool InProgress { get; private set; }
    public bool PrimaryDisabled => InProgress;
    public EditContext? EditContext { get; private set; }
    public EditCategoryFormViewModel? FormViewModel { get; private set; }

    private void FieldChanged(object? sender, FieldChangedEventArgs args) => State_StateChanged(this, EventArgs.Empty);
}
