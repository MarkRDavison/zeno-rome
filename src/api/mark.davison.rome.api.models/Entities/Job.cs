namespace mark.davison.rome.api.models.Entities;

public class Job : RomeEntity
{
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string JobRequest { get; set; } = string.Empty;
    public string JobResponse { get; set; } = string.Empty;

    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset SelectedAt { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset FinishedAt { get; set; }

    public string PerformerId { get; set; } = string.Empty;
    public Guid ContextUserId { get; set; }

    public virtual User? ContextUser { get; set; }
}