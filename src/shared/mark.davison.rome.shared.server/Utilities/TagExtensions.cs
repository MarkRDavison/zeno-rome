namespace mark.davison.rome.shared.server.Utilities;

public static class TagExtensions
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto(tag.Id, tag.Name);
    }
}
