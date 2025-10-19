using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;
using SWP391Web.Infrastructure.SignlR;
using System.Security.Claims;
using System.Text;

namespace SWP391Web.API.Extentions;
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAuthenticationService(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
          {
              options.SaveToken = true;
              options.RequireHttpsMetadata = false;
              options.TokenValidationParameters = new TokenValidationParameters()
              {
                  ValidAudience = builder.Configuration["JWT:ValidAudience"],
                  ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("Canot find JWT secret"))),
                  ValidateIssuer = true,
                  ValidateAudience = true,
                  ValidateLifetime = true,
                  ValidateIssuerSigningKey = true,
              };
          });

        return builder;
    }

    public static WebApplicationBuilder AddSwaggerGen(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "Please enter your token with format: \"Bearer YOUR_TOKEN\"",
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference()
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }

                        },
                        new List<string>()
                    }
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddHttpVNPT(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IEContractService, EContractService>(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["SmartCA:BaseUrl"]
                ?? throw new Exception("Cannot find base url in SmartCA"));

            client.Timeout = TimeSpan.FromSeconds(30);

            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.ConnectionClose = true;
        })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 1,
        AllowAutoRedirect = true,
        AutomaticDecompression = System.Net.DecompressionMethods.All,
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
    });

        return builder;
    }

    public static WebApplicationBuilder AddSignalR(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

        return builder;
    }
}
