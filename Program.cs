using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // Keep MVC if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy
            .WithOrigins("http://127.0.0.1:5500")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseWebSockets(); 

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await EchoLoop(webSocket); // Handle messages here
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

app.MapControllers();

app.Run();

static async Task EchoLoop(System.Net.WebSockets.WebSocket socket)
{
    var buffer = new byte[1024 * 4];
    var clients = new List<System.Net.WebSockets.WebSocket> { socket };

    while (socket.State == System.Net.WebSockets.WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
        {
            var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

            // Broadcast to all connected clients
            foreach (var client in clients.ToList())
            {
                if (client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    await client.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
                        System.Net.WebSockets.WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
            }
        }
        else if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
        {
            clients.Remove(socket);
            await socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}
