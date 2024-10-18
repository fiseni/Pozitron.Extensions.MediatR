using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatR;

/// <summary>
/// ExtendedMediator extensions.
/// DI extensions to scan for MediatR handlers and registers them.
/// </summary>
public static class MediatorExtensions
{
    /// <summary>
    /// Asynchronously send a notification to multiple handlers using the specified strategy.
    /// </summary>
    /// <typeparam name="TNotification"></typeparam>
    /// <param name="publisher"></param>
    /// <param name="notification">Notification object</param>
    /// <param name="strategy">Publish strategy</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    /// <exception cref="NotSupportedException">Throws if the MediatorImplementationType is not configured to ExtendedMediator</exception>
    public static Task Publish<TNotification>(
        this IPublisher publisher,
        TNotification notification,
        PublishStrategy strategy,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        return publisher is ExtendedMediator customMediator
            ? customMediator.Publish(notification, strategy, cancellationToken)
            : throw new NotSupportedException("The extended mediator implementation is not registered! Register it with the IServiceCollection.AddExtendedMediatR extensions.");
    }

    /// <summary>
    /// Registers handlers and mediator types.
    /// The MediatorImplementationType is always set to ExtendedMediator.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="lifetime">Service lifetime to register services under</param>
    /// <param name="types">Types from assemblies to scan</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        params Type[] types)
    {
        var assemblies = types.Select(x => x.Assembly).ToArray();
        return services.AddExtendedMediatR(lifetime, assemblies);
    }

    /// <summary>
    /// Registers handlers and mediator types.
    /// The MediatorImplementationType is always set to ExtendedMediator.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="types">Types from assemblies to scan</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        params Type[] types)
    {
        var assemblies = types.Select(x => x.Assembly).ToArray();
        return services.AddExtendedMediatR(assemblies);
    }

    /// <summary>
    /// Registers handlers and mediator types.
    /// The MediatorImplementationType is always set to ExtendedMediator.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="lifetime">Service lifetime to register services under</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>Service collection</returns>
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

    /// <summary>
    /// Registers handlers and mediator types.
    /// The MediatorImplementationType is always set to ExtendedMediator.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var serviceConfig = new MediatRServiceConfiguration();
        serviceConfig.RegisterServicesFromAssemblies(assemblies);

        return services.AddExtendedMediatR(serviceConfig);
    }

    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies
    /// The MediatorImplementationType is always set to ExtendedMediator.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">The action used to configure the options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration> configuration)
    {
        var serviceConfig = new MediatRServiceConfiguration();
        configuration.Invoke(serviceConfig);

        return services.AddExtendedMediatR(serviceConfig);
    }

    /// <summary>
    /// Registers handlers and mediator types from the specified assemblies.
    /// The MediatorImplementationType is always set to ExtendedMediator.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration options</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddExtendedMediatR(
        this IServiceCollection services,
        MediatRServiceConfiguration configuration)
    {
        configuration.MediatorImplementationType = typeof(ExtendedMediator);
        services.AddMediatR(configuration);

        return services;
    }
}
