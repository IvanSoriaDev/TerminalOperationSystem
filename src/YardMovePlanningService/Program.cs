using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YardMovePlanningService.Authorization;
using YardMovePlanningService.Data;
using YardMovePlanningService.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<YardDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("YardDb")));

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
    options.AddPolicy(Policies.YardRead, policy => policy.RequireClaim("scope", Policies.YardRead));
    options.AddPolicy(Policies.YardWrite, policy => policy.RequireClaim("scope", Policies.YardWrite));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<YardDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!await dbContext.YardMoveJobs.AnyAsync())
    {
        dbContext.YardMoveJobs.AddRange(
            new YardMoveJob { Id = Guid.NewGuid(), JobCode = "YARD-001", ContainerNumber = "MSCU1234567", FromLocation = "A1", ToLocation = "B3", Priority = 1, ScheduledAtUtc = DateTime.UtcNow.AddHours(1) },
            new YardMoveJob { Id = Guid.NewGuid(), JobCode = "YARD-002", ContainerNumber = "TGHU7654321", FromLocation = "C2", ToLocation = "D5", Priority = 2, ScheduledAtUtc = DateTime.UtcNow.AddHours(2) },
            new YardMoveJob { Id = Guid.NewGuid(), JobCode = "YARD-003", ContainerNumber = "CMAU9990001", FromLocation = "E1", ToLocation = "F4", Priority = 3, ScheduledAtUtc = DateTime.UtcNow.AddHours(3) });

        await dbContext.SaveChangesAsync();
    }
}

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
