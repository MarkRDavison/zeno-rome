namespace mark.davison.rome.shared.models.dto.Scenarios.Commands.UpsertCategory;

[PostRequest(Path = "upsert-category")]
public sealed class UpsertCategoryCommandRequest : ICommand<UpsertCategoryCommandRequest, UpsertCategoryCommandResponse>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
