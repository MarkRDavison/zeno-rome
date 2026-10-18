namespace mark.davison.rome.api.models.Entities;

public class UserPreference
{
    public Guid UserId { get; set; }
    public string PreferencesJson { get; set; } = "{}";

    public virtual User? User { get; set; }
}
