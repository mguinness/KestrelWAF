using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace KestrelWAF;

public class BlockIpMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<BlockIpMiddleware> logger;
    private readonly MaxMindDb geo;
    private readonly IMemoryCache cache;
    private readonly Func<WebRequest, bool> compiledRule;

    public BlockIpMiddleware(RequestDelegate next, ILogger<BlockIpMiddleware> logger, MaxMindDb geo, IMemoryCache cache,
        IOptions<Rule> ruleset)
    {
        this.next = next;
        this.logger = logger;
        this.geo = geo;
        this.cache = cache;
        this.compiledRule = new MRE().CompileRule<WebRequest>(ruleset.Value);
    }

    public async Task Invoke(HttpContext context)
    {
        var wr = new WebRequest(context.Request, geo, cache);

        if (compiledRule(wr))
        {
            logger.LogWarning($"Forbidden request from {wr.RemoteIp}");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next.Invoke(context);
    }
}