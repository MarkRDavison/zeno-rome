namespace mark.davison.rome.shared.models.dto.Scenarios.Commands.SetUserContext;


[PostRequest(Path = "set-user-context-command")]
public sealed class SetUserContextCommandRequest : ICommand<SetUserContextCommandRequest, SetUserContextCommandResponse>
{
    public DateOnly StartRange { get; set; }
    public DateOnly EndRange { get; set; }
}
