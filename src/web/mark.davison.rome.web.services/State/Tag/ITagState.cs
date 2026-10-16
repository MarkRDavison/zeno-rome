namespace mark.davison.rome.web.services.State.Tag;

public interface ITagState : IStateService
{
    IList<TagDto> Tags { get; }

    Task FetchState();
}
