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

builder.AddHttpSmartCA();

var allowedOrigins = new[] {
    "http://localhost:5173",
    "https://metrohcmc.xyz",
    "https://electricvehiclesystem.click"
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

// Seed Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Configuration.GetValue<bool>("Swagger__Enabled") || app.Environment.IsDevelopment())
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

app.UseRouting();

app.UseCors("FrontEnd");

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
