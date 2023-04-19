using System.ComponentModel.DataAnnotations;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/login", (LoginRequest loginRequest) => $"User: [{loginRequest.Username}] Logged In Successfully");

app.UseMiddleware<ErrorHandlingMiddleware>() //here we are instructing the app to use these middlewares on every request
   .UseMiddleware<RequestResponseLoggingMiddleware>(); 

//here we are instructing the app to use this middleware only on the login route
app.UseWhen(context => context.Request.Path.StartsWithSegments("/login"), appBuilder =>
{
    appBuilder.UseMiddleware<ValidationMiddleware>();
});

app.Run();


public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path;
        
        Console.WriteLine($"{path} Attempt");
        
        await _next(context); //this will call the next middleware in the chain if there is one
        
        Console.WriteLine($"{path} Success");
    }
}

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering(); //this will enable us to read the request body more than once
        var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
        
        //since the body is a stream, we need to rewind it to the beginning in order for the next middleware or controller to read it again
        context.Request.Body.Position = 0;  

        LoginRequest request = System.Text.Json.JsonSerializer.Deserialize<LoginRequest>(bodyAsText);

        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            throw new ValidationException("Invalid Login Request");

        await _next(context);
    }
}

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            //the only job of this middleware is to be a catch all and not let any exception details returned to the caller.
            await _next(context);
        }
        catch (ValidationException e)
        {
            Console.WriteLine(e);

            //instead of letting the exception bubble up to the caller, we are writing a server log and returning the correct http response code
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}