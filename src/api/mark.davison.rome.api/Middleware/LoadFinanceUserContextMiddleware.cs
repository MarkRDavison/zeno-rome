namespace mark.davison.rome.api.Middleware;

public class LoadFinanceUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public LoadFinanceUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IFinanceUserContext financeUserContext)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            await financeUserContext.LoadAsync(context.RequestAborted);
        }

        await _next(context);
    }
}