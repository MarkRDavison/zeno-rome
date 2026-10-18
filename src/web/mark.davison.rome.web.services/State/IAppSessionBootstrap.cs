namespace mark.davison.rome.web.services.State;

public interface IAppSessionBootstrap
{
    Task EnsureAuthenticatedStartupAsync(CancellationToken cancellationToken = default);
}
