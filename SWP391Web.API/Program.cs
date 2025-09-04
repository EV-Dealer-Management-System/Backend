using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SWP391Web.API.Extentions;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();

// Register SwaggerGen and config for Authorize
// Base on Extensions.WebApplicationBuilderExtensions
builder.AddSwaggerGen();

// Register Authentication
// Base on Extensions.WebApplicationBuilderExtensions
builder.AddAuthenticationService();
builder.Services.AddAuthorization();

// Register services life cycle
// Base on Extensions.ServiceCollectionExtensions
builder.Services.RegisterService();

// Register redis services
// Base on Extensions.RedisServiceExtensions
builder.AddRedisCacheService();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
