namespace mark.davison.rome.web.components.Pages.Category.Create;

public partial class CreateCategory : BaseView<EditCategoryViewModel, Guid?>
{
    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.Initialize(null);
    }
}
