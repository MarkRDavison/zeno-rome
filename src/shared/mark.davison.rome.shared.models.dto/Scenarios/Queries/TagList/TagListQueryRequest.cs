namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.TagList;

[GetRequest(Path = "tag-list-query")]
public sealed class TagListQueryRequest : IQuery<TagListQueryRequest, TagListQueryResponse>
{
}
