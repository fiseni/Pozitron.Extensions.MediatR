using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatR;

internal class ExtendedMediator(
    IServiceScopeFactory serviceScopeFactory,
    IServiceProvider serviceProvider)
    : Mediator(serviceProvider)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private static readonly Dictionary<int, INotificationPublisher> _publishers = new()
    {
        [(int)PublishStrategy.Sequential] = new SequentialPublisher(),
        [(int)PublishStrategy.SequentialAll] = new SequentialAllPublisher(),
        [(int)PublishStrategy.WhenAll] = new WhenAllPublisher(),
    };

    public Task Publish<TNotification>(
        TNotification notification,
        PublishStrategy strategy,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var noWaitMode = (int)strategy > 10;
        var key = noWaitMode
            ? (int)strategy - 10
            : (int)strategy;

        if (_publishers.TryGetValue(key, out var publisher))
        {
            return noWaitMode
                ? PublishNoWait(_serviceScopeFactory, notification, publisher, cancellationToken)
                : Publish(_serviceProvider, notification, publisher, cancellationToken);
        }

        return Publish(notification, cancellationToken);
    }

    private static Task Publish<TNotification>(
        IServiceProvider serviceProvider,
        TNotification notification,
        INotificationPublisher publisher,
        CancellationToken cancellationToken) where TNotification : INotification
        => new Mediator(serviceProvider, publisher).Publish(notification, cancellationToken);

    private static Task PublishNoWait<TNotification>(
        IServiceScopeFactory serviceScopeFactory,
        TNotification notification,
        INotificationPublisher publisher,
        CancellationToken cancellationToken) where TNotification : INotification
    {
        _ = Task.Run(async () =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<ExtendedMediator>>();

            try
            {
                var mediator = new Mediator(scope.ServiceProvider, publisher);
                await mediator.Publish(notification, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // The aggregate exceptions are already flattened by the publishers.
                logger?.LogError(ex, "Error occurred while executing the handler in NoWait mode!");
            }

        }, cancellationToken);

        return Task.CompletedTask;
    }
}
