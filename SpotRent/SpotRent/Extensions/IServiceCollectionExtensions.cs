using SpotRent.Implementations;
using SpotRent.Interfaces;

namespace SpotRent.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
