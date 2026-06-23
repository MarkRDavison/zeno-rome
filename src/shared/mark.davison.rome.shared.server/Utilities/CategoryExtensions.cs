namespace mark.davison.rome.shared.server.Utilities;

public static class CategoryExtensions
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto(category.Id, category.Name);
    }
}
