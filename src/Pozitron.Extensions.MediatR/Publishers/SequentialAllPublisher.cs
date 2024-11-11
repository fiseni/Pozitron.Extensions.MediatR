namespace MediatR;

internal class SequentialAllPublisher : INotificationPublisher
{
    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        List<Exception>? exceptions = null;

        foreach (var handlerExecutor in handlerExecutors)
        {
            try
            {
                await handlerExecutor.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                (exceptions ??= []).AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex)
            {
                (exceptions ??= []).Add(ex);
            }
        }

        if (exceptions?.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}
