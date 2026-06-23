namespace mark.davison.rome.web.components.Pages.Category.List;

public partial class CategoryList : BaseView<CategoryListViewModel, object?>
{
    protected override async Task<bool> InitializeViewModel()
    {
        return await ViewModel.Initialize(null);
    }
}
