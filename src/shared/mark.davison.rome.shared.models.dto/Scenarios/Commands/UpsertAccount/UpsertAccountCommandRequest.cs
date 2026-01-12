namespace mark.davison.rome.shared.models.dto.Scenarios.Commands.UpsertAccount;

[PostRequest(Path = "upsert-user-account")]
public sealed class UpsertAccountCommandRequest : ICommand<UpsertAccountCommandRequest, UpsertAccountCommandResponse>
{
    public UpsertAccountDto UpsertAccountDto { get; set; } = UpsertAccountDto.Default;
}