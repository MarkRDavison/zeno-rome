namespace mark.davison.rome.api.commands.Scenarios.SetUserContext;

public sealed class SetUserContextCommandValidator : ICommandValidator<SetUserContextCommandRequest, SetUserContextCommandResponse>
{
    public const string VALIDATION_INVALID_DATE_RANGE = "VALIDATION_INVALID_DATE_RANGE";

    public Task<SetUserContextCommandResponse> ValidateAsync(
        SetUserContextCommandRequest request,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        if (request.StartRange > request.EndRange)
        {
            return Task.FromResult(new SetUserContextCommandResponse
            {
                Errors = [VALIDATION_INVALID_DATE_RANGE]
            });
        }

        return Task.FromResult(new SetUserContextCommandResponse());
    }
}
