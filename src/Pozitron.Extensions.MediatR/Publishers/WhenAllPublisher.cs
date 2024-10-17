namespace MediatR;

internal class WhenAllPublisher : INotificationPublisher
{
    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        List<Exception>? exceptions = null;
        var tasks = new List<Task>();

        // Some of the tasks may throw an immediate exception, synchronously, and we can't guarantee they're utilizing async state machine.
        // e.g. public Task Handle(CancellationToken cancellationToken) => throw new Exception();
        // In that case, WhenAll won't even be executed. So, we must iterate and catch these exceptions beforehand.
        foreach (var handlerExecutor in handlerExecutors)
        {
            try
            {
                tasks.Add(handlerExecutor.HandlerCallback(notification, cancellationToken));
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

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch
        {
            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    if (task.Exception.InnerExceptions.Count > 1 || task.Exception.InnerException is AggregateException)
                    {
                        (exceptions ??= []).AddRange(task.Exception.Flatten().InnerExceptions);
                    }
                    else if (task.Exception.InnerException is not null)
                    {
                        (exceptions ??= []).Add(task.Exception.InnerException);
                    }
                }
                else if (task.IsCanceled)
                {
                    try
                    {
                        // This will force the task to throw the exception if it's canceled.
                        // This will preserve all the information compared to creating a new TaskCanceledException manually.
                        task.GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        (exceptions ??= []).Add(ex);
                    }
                }
            }
        }

        if (exceptions is not null && exceptions.Count != 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}
