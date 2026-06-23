namespace mark.davison.rome.web.components.Pages.Category.List;

// TODO: BaseViewModel -> needs non generic version
public sealed class CategoryListViewModel : BaseViewModel<object?>
{
    private bool _isLoading = true;
    private readonly ICategoryState _categoryState;

    public CategoryListViewModel(
        IAppContextService appContextService,
        ICategoryState categoryState
    ) : base(
        appContextService)
    {
        _categoryState = categoryState;

        RegisterState(_categoryState);
    }

    public override async Task<bool> Initialize(object? payload)
    {
        _isLoading = true;

        await _categoryState.FetchState();

        _isLoading = false;

        return true;
    }

    public bool Loading => _isLoading || IsStateLoading;

    public IEnumerable<CategoryListItemViewModel> Items => _categoryState.Categories.Select(ToListItemViewModel);

    private CategoryListItemViewModel ToListItemViewModel(CategoryDto category)
    {
        return new CategoryListItemViewModel
        {
            Id = category.Id,
            Name = new LinkDefinition
            {
                Href = RouteHelpers.Category(category.Id),
                Text = category.Name
            }
        };
    }

    public List<CommandMenuItem> CommandMenuItems { get; set; } =
    [
        new CommandMenuItem{ Text = "Edit", Id = "EDIT" },
        new CommandMenuItem{ Text = "Delete", Id = "DELETE" }
    ];

    public void AddCategory()
    {
        throw new NotImplementedException();
    }
}
