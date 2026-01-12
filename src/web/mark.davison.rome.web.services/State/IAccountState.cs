namespace mark.davison.rome.web.services.State;

public interface IAccountState : IStateService
{
    void SetState(IList<AccountDto> accounts);

    Task FetchState(Guid? accountTypeId);

    IList<AccountDto> Accounts { get; }
}
