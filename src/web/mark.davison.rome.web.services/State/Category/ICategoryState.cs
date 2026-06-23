namespace mark.davison.rome.web.services.State.Category;

public interface ICategoryState : IStateService
{
    IList<CategoryDto> Categories { get; }

    Task FetchState();
}
