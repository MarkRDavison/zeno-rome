namespace mark.davison.rome.web.components.Pages.UserSettings;

public partial class UserSettingsView : BaseView<UserSettingsViewModel, object?>
{
    protected override Task<bool> InitializeViewModel() => ViewModel.Initialize(null);
}
