using System.Globalization;

namespace IrelandFinanceApp.Middlewares;

public class UserCultureMiddleware
{
    private readonly RequestDelegate _next;

    public UserCultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var cultureClaim = context.User.FindFirst("CultureCode")?.Value;

            if (!string.IsNullOrWhiteSpace(cultureClaim))
            {
                try
                {
                    var culture = new CultureInfo(cultureClaim);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                }
                catch (CultureNotFoundException)
                {
                    var fallback = new CultureInfo("en-IE");
                    CultureInfo.CurrentCulture = fallback;
                    CultureInfo.CurrentUICulture = fallback;
                }
            }
        }

        await _next(context);
    }
}