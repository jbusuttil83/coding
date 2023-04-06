var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/login", (LoginRequest loginRequest) => $"User: [{loginRequest.Username}] Logged In Successfully");

app.UseMiddleware<RequestResponseLoggingMiddleware>()
   .UseMiddleware<ValidationMiddleware>();

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
        Console.WriteLine("Login Attempt");
        
        await _next(context); //this will call the next middleware in the chain if there is one
        
        Console.WriteLine("Login Success");
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
            throw new Exception("Invalid Login Request");

        await _next(context);

    }
}