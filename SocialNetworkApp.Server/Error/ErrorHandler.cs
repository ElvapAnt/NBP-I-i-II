using System.Reflection;

namespace SocialNetworkApp.Server.Error;

public class ErrorHandler(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(CustomException e)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            //var errorMessage = new { error = e.Message };
            await context.Response.WriteAsync(e.Message);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            //var errorMessage = new { error =ex.Message };
            await context.Response.WriteAsync(ex.Message);
        }
    }
}