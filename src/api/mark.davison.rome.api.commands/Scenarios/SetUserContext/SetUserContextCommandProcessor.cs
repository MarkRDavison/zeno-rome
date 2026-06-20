namespace mark.davison.rome.api.commands.Scenarios.SetUserContext;

public sealed class SetUserContextCommandProcessor : ICommandProcessor<SetUserContextCommandRequest, SetUserContextCommandResponse>
{
    private readonly IFinanceUserContext _financeUserContext;

    public SetUserContextCommandProcessor(IFinanceUserContext financeUserContext)
    {
        _financeUserContext = financeUserContext;
    }

    public async Task<SetUserContextCommandResponse> ProcessAsync(SetUserContextCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        await _financeUserContext.SetAsync(request.StartRange, request.EndRange, cancellationToken);

        return new SetUserContextCommandResponse
        {
            Value = new UserContextDto(request.StartRange, request.EndRange)
        };
    }
}
