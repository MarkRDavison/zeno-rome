namespace mark.davison.rome.shared.models.dto.Scenarios.Commands.UpsertTag;

[PostRequest(Path = "upsert-tag")]
public sealed class UpsertTagCommandRequest : ICommand<UpsertTagCommandRequest, UpsertTagCommandResponse>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
