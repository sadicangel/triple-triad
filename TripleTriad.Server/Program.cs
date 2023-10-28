using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using TripleTriad.Services;
using TripleTriad.Users.Commands;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR(opts => opts.EnableDetailedErrors = true)
    .AddJsonProtocol(opts => opts.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)
    .AddMessagePackProtocol();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.RequireHttpsMetadata = false;
        opts.SaveToken = true;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetRequiredSection("SecurityKey").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddDomain(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<TripleTriadHub>("/tripletriad")
    .RequireAuthorization();

app.MapPost("/register", async (ISender mediator, RegisterUserCommand command, CancellationToken cancellationToken) => await mediator.Send(command, cancellationToken));
app.MapPost("/login", async (ISender mediator, LoginUserCommand command, CancellationToken cancellationToken) => await mediator.Send(command, cancellationToken));

app.Run();
