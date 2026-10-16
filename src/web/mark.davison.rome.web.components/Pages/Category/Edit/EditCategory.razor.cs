namespace mark.davison.rome.web.components.Pages.Category.Edit;

public partial class EditCategory : BaseView<EditCategoryViewModel, Guid?>
{
    [Parameter]
    public Guid Id { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.Initialize(Id);
    }
}
