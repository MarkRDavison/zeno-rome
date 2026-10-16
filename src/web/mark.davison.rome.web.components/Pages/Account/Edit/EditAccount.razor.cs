namespace mark.davison.rome.web.components.Pages.Account.Edit;

public partial class EditAccount : BaseView<EditAccountViewModel, (Guid, EditAccountFormViewModel?)>
{
    [Parameter]
    public required Guid Id { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.InitializeForEdit(Id);
    }
}
