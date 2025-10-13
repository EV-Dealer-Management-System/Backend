using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.IService;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Mappings;
using SWP391Web.Application.Service;
using SWP391Web.Application.Services;
using SWP391Web.Application.Validations;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using SWP391Web.Infrastructure.Repository;

namespace SWP391Web.API.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterService(this IServiceCollection services)
        {
            // Register Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<IDashBoardService, DashBoardService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IEContractService, EContractService>();
            services.AddScoped<IGHNService, GHNService>();
            services.AddScoped<IElectricVehicleColorService, ElectricVehicleColorService>();
            services.AddScoped<IElectricVehicleModelService, ElectricVehicleModelService>();
            services.AddScoped<IElectricVehicleVersionService, ElectricVehicleVersionService>();
            services.AddScoped<IElectricVehicleService, ElectricVehicleService>();
            services.AddScoped<IBookingEVService, BookingEVService>();
            services.AddScoped<IEVCInventoryService, EVCInventoryService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IEContractTemplateService, EContractTemplateService>();
            services.AddScoped<IS3Service, S3Service>();

            // Register Infrastructure Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IVnptEContractClient, VnptEContractClient>();
            services.AddScoped<IGHNClient, GHNClient>();

            // Register Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register Fluent Validation
            services.AddValidatorsFromAssemblyContaining<RegisterCustomerValidation>();
            services.AddValidatorsFromAssemblyContaining<LoginUserValidation>();
            services.AddValidatorsFromAssemblyContaining<ForgotPasswordValidation>();
            services.AddValidatorsFromAssemblyContaining<ResetPasswordValidation>();

            services.AddFluentValidationAutoValidation();

            // Configure token lifespan for email confirmation
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(1)
            );


            services.AddAutoMapper(cfg => { }, typeof(AutoMappingProfile));

            // Remove InvalidModelState, keep fluent validation
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors
                                .Where(e => !e.ErrorMessage.StartsWith("The ") || !e.ErrorMessage.EndsWith(" field is required."))
                                .Select(e => e.ErrorMessage)
                                .Where(msg => !string.IsNullOrEmpty(msg))
                                .ToArray()
                        )
                        .Where(kvp => kvp.Value.Length > 0)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    var result = new
                    {
                        type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                        title = "One or more validation errors occurred.",
                        status = 400,
                        errors = errors,
                        traceId = context.HttpContext.TraceIdentifier
                    };

                    return new BadRequestObjectResult(result);
                };
            });

            return services;
        }
    }
}
