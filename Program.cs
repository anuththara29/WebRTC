using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();  // Add MVC Controllers
builder.Services.AddSignalR();  // Add SignalR services

// Add CORS policy to allow frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy
            .WithOrigins("http://127.0.0.1:5500")  
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Add authorization services
builder.Services.AddAuthorization(); 

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use CORS with the policy defined 
app.UseCors("AllowAllOrigins");

// Use routing for controllers and SignalR endpoints
app.UseRouting();

// Add the authorization middleware
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub route
app.MapHub<ChatHub>("/chathub");

app.Run();
