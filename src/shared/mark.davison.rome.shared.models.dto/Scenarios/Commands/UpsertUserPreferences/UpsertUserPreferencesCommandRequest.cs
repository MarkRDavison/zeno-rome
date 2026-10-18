namespace mark.davison.rome.shared.models.dto.Scenarios.Commands.UpsertUserPreferences;

[PostRequest(Path = "upsert-user-preferences")]
public sealed class UpsertUserPreferencesCommandRequest : ICommand<UpsertUserPreferencesCommandRequest, UpsertUserPreferencesCommandResponse>
{
    public UserPreferencesDto Preferences { get; set; } = new();
}
