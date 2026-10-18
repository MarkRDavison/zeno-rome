namespace mark.davison.rome.api.commands.Scenarios.UpsertUserPreferences;

public sealed class UpsertUserPreferencesCommandValidator : ICommandValidator<UpsertUserPreferencesCommandRequest, UpsertUserPreferencesCommandResponse>
{
    public const string VALIDATION_HOME_RECENT_COUNT_OUT_OF_RANGE =
        "VALIDATION_HOME_RECENT_COUNT_OUT_OF_RANGE${0}${1}";

    public Task<UpsertUserPreferencesCommandResponse> ValidateAsync(
        UpsertUserPreferencesCommandRequest request,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var count = request.Preferences.HomeRecentTransactionCount;
        if (count < UserPreferenceConstants.HomeRecentTransactionCountMin ||
            count > UserPreferenceConstants.HomeRecentTransactionCountMax)
        {
            return Task.FromResult(new UpsertUserPreferencesCommandResponse
            {
                Errors =
                [
                    string.Format(
                        VALIDATION_HOME_RECENT_COUNT_OUT_OF_RANGE,
                        UserPreferenceConstants.HomeRecentTransactionCountMin,
                        UserPreferenceConstants.HomeRecentTransactionCountMax)
                ]
            });
        }

        return Task.FromResult(new UpsertUserPreferencesCommandResponse());
    }
}
