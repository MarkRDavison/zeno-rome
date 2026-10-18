using System.Text.Json;
using mark.davison.rome.shared.constants;

namespace mark.davison.rome.api.persistence.UserPreferences;

public static class UserPreferencesSupport
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static UserPreferencesDto CreateDefault() =>
        new()
        {
            HomeRecentTransactionCount = UserPreferenceConstants.HomeRecentTransactionCountDefault
        };

    public static UserPreferencesDto Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateDefault();
        }

        try
        {
            var dto = JsonSerializer.Deserialize<UserPreferencesDto>(json, JsonOptions);
            return dto is null ? CreateDefault() : Normalize(dto);
        }
        catch (JsonException)
        {
            return CreateDefault();
        }
    }

    public static string Serialize(UserPreferencesDto preferences) =>
        JsonSerializer.Serialize(Normalize(preferences), JsonOptions);

    public static UserPreferencesDto Normalize(UserPreferencesDto preferences)
    {
        var count = preferences.HomeRecentTransactionCount;
        if (count < UserPreferenceConstants.HomeRecentTransactionCountMin)
        {
            count = UserPreferenceConstants.HomeRecentTransactionCountMin;
        }
        else if (count > UserPreferenceConstants.HomeRecentTransactionCountMax)
        {
            count = UserPreferenceConstants.HomeRecentTransactionCountMax;
        }

        return new UserPreferencesDto { HomeRecentTransactionCount = count };
    }

    public static async Task<UserPreferencesDto> LoadAsync(
        IDbContext<RomeDbContext> dbContext,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var row = await dbContext
            .Set<UserPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(_ => _.UserId == userId, cancellationToken);

        return row is null ? CreateDefault() : Deserialize(row.PreferencesJson);
    }
}
