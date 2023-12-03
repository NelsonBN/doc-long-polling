using System.Collections.Concurrent;
using System.Diagnostics;

ConcurrentQueue<string> queue = new();

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials()));

var app = builder.Build();

app.UseCors();

app.MapGet("receiver", async context =>
{
    try
    {
        Debug.WriteLine("[REQUEST] Starting...");

        var response = context.Response.StatusCode = 200;
        if(queue.TryDequeue(out var message))
        {
            Debug.WriteLine($"[REQUEST] {message}");
            await context.Response.WriteAsync(message);
        }
        else
        {
            Debug.WriteLine("[REQUEST] No message");
            await context.Response.WriteAsync("No message");
        }
    }
    catch(Exception exception)
    {
        Debug.WriteLine($"[REQUEST] {exception.Message}");
    }

    Debug.WriteLine("[REQUEST] finished");
});

await Task.WhenAll(
    Writer(),
    app.RunAsync());

async Task Writer()
{
    while(true)
    {
        var message = $"Message {DateTime.Now}";
        queue.Enqueue(message);
        Debug.WriteLine($"[WRITER] {message}");
        await Task.Delay(1500);
    }
}
