var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSignalR(opts => opts.EnableDetailedErrors = builder.Environment.IsDevelopment())
    .AddMessagePackProtocol();

var app = builder.Build();

app.MapHub<ServerHub>("/triple-triad");

if (app.Environment.IsDevelopment())
    app.MapGet("/", () => "Triple Triad server running!");

app.Run();
