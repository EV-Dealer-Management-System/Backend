using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SWP391Web.API.Extentions;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5000);
//    options.ListenAnyIP(5000);
//});

//builder.Configuration.AddSystemsManager("/swp391/prod/", reloadAfter: TimeSpan.FromMinutes(5));
//builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
//builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));

QuestPDF.Settings.License = LicenseType.Community;


// Add services to the container.

builder.Services.AddControllers();

//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();

// Register services
// Base on Extensions.ServiceCollectionExtensions
builder.Services.RegisterService();

// Register Authentication service
// Base on Extensions.WebApplicationBuilderExtensions
builder.AddAuthenticationService();

// Register Swagger that has bearer services
// Base on Extensions.ServiceCollectionExtensions
builder.AddSwaggerGen();

builder.AddRedisCacheService();

builder.AddHttpVNPT();

var allowedOrigins = new[] {
    "http://localhost:5173",
    "https://metrohcmc.xyz",
    "https://electricvehiclesystem.click",
    "https://api.electricvehiclesystem.click"
};

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("FrontEnd", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
    );
});


var app = builder.Build();

var forwardOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardOptions.KnownNetworks.Clear();
forwardOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardOptions);

app.UseHttpsRedirection();

if (app.Configuration.GetValue<bool>("Swagger:Enabled") || app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Method == HttpMethods.Head &&
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Request.Method = HttpMethods.Get;
        }
        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseRouting();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Pipeline error: {ex.Message}");
        throw;
    }
});

app.UseCors("FrontEnd");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}

app.Run();
