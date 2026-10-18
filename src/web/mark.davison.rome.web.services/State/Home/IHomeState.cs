using mark.davison.rome.shared.models.dto.Scenarios.Queries.Home;

namespace mark.davison.rome.web.services.State.Home;

public interface IHomeState : IStateService
{
    HomeQueryValue? Home { get; }
    Task FetchState();
}
