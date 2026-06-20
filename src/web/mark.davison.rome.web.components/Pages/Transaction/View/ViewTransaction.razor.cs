namespace mark.davison.rome.web.components.Pages.Transaction.View;

public partial class ViewTransaction : BaseView<ViewTransactionViewModel, Guid>
{
    [Parameter]
    public required Guid Id { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.Initialize(Id);
    }
}
