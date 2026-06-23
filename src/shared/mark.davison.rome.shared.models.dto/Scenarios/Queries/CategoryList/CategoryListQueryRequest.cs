namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.CategoryList;

[GetRequest(Path = "category-list-query")]
public sealed class CategoryListQueryRequest : IQuery<CategoryListQueryRequest, CategoryListQueryResponse>
{
}
