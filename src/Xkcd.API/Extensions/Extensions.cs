using Xkcd.API.Infrastructure.Interceptors;
using XkcdApi;

namespace Xkcd.API.Extensions;

public static class Extensions
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddTransient<GrpcExceptionInterceptor>();

        services.AddGrpcClient<XkcdService.XkcdServiceClient>((services, options) =>
        {
            options.Address = new Uri("http://localhost:5001");
        }).AddInterceptor<GrpcExceptionInterceptor>();
        
        return services;
    }
}