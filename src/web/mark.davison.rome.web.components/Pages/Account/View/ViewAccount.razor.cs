namespace mark.davison.rome.web.components.Pages.Account.View;

public partial class ViewAccount : BaseView<ViewAccountViewModel, Guid>
{
    [Parameter]
    public required Guid Id { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.Initialize(Id);
    }

    private string GetAccountCurrentBalanceCssClasses()
    {
        return $"account-current-balance {(ViewModel.Account is null ? string.Empty : AmountColourUtilities.GetAmountColourClass(ViewModel.Account.CurrentBalance))}";
    }
}
