using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatR;

public static class MediatorExtensions
{
    public static Task Publish<TNotification>(
        this IPublisher publisher,
        TNotification notification,
        PublishStrategy strategy,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        return publisher is ExtendedMediator customMediator
            ? customMediator.Publish(notification, strategy, cancellationToken)
            : throw new NotSupportedException("The extended mediator implementation is not registered! Register it with the IServiceCollection.AddExtendedMediatR extensions.");
    }

    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        params Type[] handlerAssemblyMarkerTypes)
    {
        var assemblies = handlerAssemblyMarkerTypes.Select(x => x.Assembly).ToArray();
        return services.AddExtendedMediatR(lifetime, assemblies);
    }

    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        params Type[] handlerAssemblyMarkerTypes)
    {
        var assemblies = handlerAssemblyMarkerTypes.Select(x => x.Assembly).ToArray();
        return services.AddExtendedMediatR(assemblies);
    }

    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        params Assembly[] assemblies)
    {
        var serviceConfig = new MediatRServiceConfiguration();
        serviceConfig.Lifetime = lifetime;
        serviceConfig.RegisterServicesFromAssemblies(assemblies);

        return services.AddExtendedMediatR(serviceConfig);
    }

    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var serviceConfig = new MediatRServiceConfiguration();
        serviceConfig.RegisterServicesFromAssemblies(assemblies);

        return services.AddExtendedMediatR(serviceConfig);
    }

    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration> configuration)
    {
        var serviceConfig = new MediatRServiceConfiguration();
        configuration.Invoke(serviceConfig);

        return services.AddExtendedMediatR(serviceConfig);
    }

    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        MediatRServiceConfiguration configuration)
    {
        configuration.MediatorImplementationType = typeof(ExtendedMediator);
        services.AddMediatR(configuration);

        return services;
    }
}
