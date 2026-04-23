using System.Text;
using ContainerOperationsService.Authorization;
using ContainerOperationsService.Data;
using ContainerOperationsService.Domain;
using ContainerOperationsService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContainerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContainerDb")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.ContainersRead, policy => policy.RequireClaim("scope", Policies.ContainersRead));
    options.AddPolicy(Policies.ContainersWrite, policy => policy.RequireClaim("scope", Policies.ContainersWrite));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste the JWT access token returned by Auth Service.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(),
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ContainerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Containers.AnyAsync())
        {
            dbContext.Containers.AddRange(
                new ContainerUnit { Id = Guid.NewGuid(), ContainerNumber = "MSCU1234567", Status = "inbound", LastUpdatedUtc = DateTime.UtcNow },
                new ContainerUnit { Id = Guid.NewGuid(), ContainerNumber = "TGHU7654321", Status = "hold", LastUpdatedUtc = DateTime.UtcNow },
                new ContainerUnit { Id = Guid.NewGuid(), ContainerNumber = "CMAU9990001", Status = "loaded", LastUpdatedUtc = DateTime.UtcNow });

            await dbContext.SaveChangesAsync();
        }
    }
    catch (Exception exception)
    {
        app.Logger.LogCritical(exception, "An error occurred while initializing Container Operations Service data.");
        throw;
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
