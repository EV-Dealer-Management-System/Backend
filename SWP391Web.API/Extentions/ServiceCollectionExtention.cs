using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using SWP391Web.Application.IService;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Mappings;
using SWP391Web.Application.Service;
using SWP391Web.Application.Services;
using SWP391Web.Application.Validation;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using SWP391Web.Infrastructure.Repository;

namespace SWP391Web.API.Extentions
{
    public static class ServiceCollectionExtention
    {
        public static IServiceCollection RegisterService(this IServiceCollection services)
        {
            // Register Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRedisService, RedisService>();

            // Register Infrastructure Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register Fluent Validation
            services.AddValidatorsFromAssemblyContaining<RegisterCustomerValidation>();
            services.AddValidatorsFromAssemblyContaining<LoginUserValidation>();

            services.AddFluentValidationAutoValidation();

            // Configure token lifespan for email confirmation
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(1)
            );


            services.AddAutoMapper(cfg => { }, typeof(AutoMappingProfile));

            return services;
        }
    }
}
