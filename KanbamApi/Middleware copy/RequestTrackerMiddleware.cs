using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Util;

namespace KanbamApi.Middleware;

public class RequestTrackerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IVisitorsRepo _visitorsRepo;

    public RequestTrackerMiddleware(IVisitorsRepo visitorsRepo, RequestDelegate next)
    {
        _next = next;
        _visitorsRepo = visitorsRepo;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
        var userId = context.User?.FindFirst("userId")?.Value;

        var visitorDetail = new Visitor()
        {
            IpAddress = context.Connection.RemoteIpAddress?.ToString()!,
            Timestamp = [DateTime.UtcNow],
            HttpMethod = [request.Method],
            Path = [request.Path],
            UserAgent = [request.Headers["User-Agent"].ToString()],
            UserId = userId
        };

        CreateVisitorRecord(visitorDetail, userId!);

        await _next(context);
    }

    private async void CreateVisitorRecord(Visitor visitorDetail, string userId)
    {
        if (userId is not null)
        {
            var visitor = await _visitorsRepo.GetByUserIdAsync(userId);

            if (visitor.Count != 0)
            {
                MiddlewareHelpers.UpdateVisitor(visitorDetail, visitor);
                await _visitorsRepo.UpdateAsync(visitorDetail, userId);
            }
            else
                await _visitorsRepo.CreateAsync(visitorDetail);
        }
        else
            await _visitorsRepo.CreateAsync(visitorDetail);
    }
}
