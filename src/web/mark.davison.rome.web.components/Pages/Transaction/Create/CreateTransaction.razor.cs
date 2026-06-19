namespace mark.davison.rome.web.components.Pages.Transaction.Create;

public partial class CreateTransaction : BaseView<EditTransactionViewModel, (Guid, EditTransactionFormViewModel?)>
{
    [Parameter]
    public required Guid Type { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.Initialize((Type, null));
    }
}
