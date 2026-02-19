using Hangfire.Dashboard;

namespace ToDoList.API.Filters;

public class HangfireAuthorizationFilter(bool isDevelopment) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (isDevelopment)
        {
            return true;
        }

        // In production, require authentication
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
