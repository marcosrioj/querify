using Querify.Tenant.BackOffice.Business.User.Abstractions;
using Querify.Tenant.BackOffice.Business.User.Commands.CreateUser;
using Querify.Tenant.BackOffice.Business.User.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.BackOffice.Business.User.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserBusiness(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<UsersCreateUserCommandHandler>());

        return services;
    }
}