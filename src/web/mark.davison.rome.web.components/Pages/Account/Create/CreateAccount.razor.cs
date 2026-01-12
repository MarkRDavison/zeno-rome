namespace mark.davison.rome.web.components.Pages.Account.Create;

public partial class CreateAccount : BaseView<EditAccountViewModel, (Guid, EditAccountFormViewModel?)>
{
    [Parameter]
    public required Guid Type { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.Initialize((Type, null));
    }
}
