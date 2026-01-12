namespace mark.davison.rome.web.components.Pages.Account.Edit;

public partial class EditAccount : BaseView<EditAccountViewModel, (Guid, EditAccountFormViewModel?)>
{
    [Parameter]
    public required Guid Id { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        Console.Error.WriteLine("TODO: Need to fetch the instance to pass through");
        return ViewModel.Initialize((Guid.Empty, null));
    }
}
