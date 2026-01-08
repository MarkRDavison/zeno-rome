namespace mark.davison.rome.shared.models.dto.Scenarios.Queries.Startup;

[GetRequest(AllowAnonymous = true, Path = "bootstrap-query")]
public sealed class BootstrapQueryRequest : IQuery<BootstrapQueryRequest, BootstrapQueryResponse>;