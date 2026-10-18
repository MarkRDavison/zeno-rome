namespace mark.davison.rome.web.components.Pages.Dashboard;

public partial class DashboardView : BaseView<DashboardViewModel, object?>
{
    protected override Task<bool> InitializeViewModel() => ViewModel.Initialize(null);
}
