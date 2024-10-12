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

    private static readonly Dictionary<PublishStrategy, (INotificationPublisher Publisher, bool NoWaitMode)> _publishers = new()
    {
        [PublishStrategy.Sequential] = (new SequentialPublisher(), false),
        [PublishStrategy.SequentialAll] = (new SequentialAllPublisher(), false),
        [PublishStrategy.WhenAll] = (new WhenAllPublisher(), false),
        [PublishStrategy.SequentialAllNoWait] = (new SequentialAllPublisher(), true),
        [PublishStrategy.WhenAllNoWait] = (new WhenAllPublisher(), true)
    };

    public Task Publish<TNotification>(
        TNotification notification,
        PublishStrategy strategy,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        if (strategy == PublishStrategy.Default)
        {
            return Publish(notification, cancellationToken);
        }

        var (publisher, noWaitMode) = _publishers[strategy];

        return noWaitMode
            ? PublishNoWait(_serviceScopeFactory, notification, publisher, cancellationToken)
            : Publish(_serviceProvider, notification, publisher, cancellationToken);
    }

    private static Task Publish<TNotification>(
        IServiceProvider serviceProvider,
        TNotification notification,
        INotificationPublisher publisher,
        CancellationToken cancellationToken) where TNotification : INotification
    {
        return new Mediator(serviceProvider, publisher).Publish(notification, cancellationToken);
    }

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
                logger?.LogError(ex, "Error occurred while executing the handler in NoWait mode!");
            }

        }, cancellationToken);

        return Task.CompletedTask;
    }
}
