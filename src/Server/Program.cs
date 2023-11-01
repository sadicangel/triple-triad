using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TripleTriad;
using TripleTriad.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDomain();

if (builder.Environment.IsDevelopment())
    builder.Services.AddTransient<DbMigratorDev>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<User>(opts => opts.User.RequireUniqueEmail = true)
    .AddEntityFrameworkStores<DataContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ServerManager>();

builder.Services.AddSignalR(opts => opts.EnableDetailedErrors = builder.Environment.IsDevelopment())
    .AddMessagePackProtocol();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var dbMigrator = app.Services.GetRequiredService<DbMigratorDev>();
    await dbMigrator.MigrateAsync(ensureDeleted: false);
}

//var options = app.Services.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
//if (options.Host == "localhost")
//{
//    var connectionString = options.ConnectionString;
//    var startInfo = new ProcessStartInfo
//    {
//        FileName = "efbundle.exe",
//        Arguments = $"--connection {connectionString}",
//        RedirectStandardOutput = false,
//        RedirectStandardError = true,
//    };
//    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start DB migration process");
//    await process.WaitForExitAsync();
//    if (process.ExitCode != 0)
//        throw new InvalidOperationException(process.StandardError.ReadToEnd());
//}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>("/triple-triad")
    .RequireAuthorization();

app.MapIdentityApi<User>();
app.Run();

file sealed class DbMigratorDev(IOptions<PgSqlOptions> pgSqlOptions)
{
    public async Task MigrateAsync(bool ensureDeleted)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql((pgSqlOptions.Value with { Database = "postgres" }).ConnectionString)
            .UseLoggerFactory(LoggerFactory.Create(opts => opts.AddConsole()))
            .EnableSensitiveDataLogging()
            .Options;

        using var dataContext = new DataContext(options);
        if (ensureDeleted)
            await dataContext.Database.EnsureDeletedAsync();
        await dataContext.Database.EnsureCreatedAsync();
    }
}