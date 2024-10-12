namespace MediatR;

public enum PublishStrategy
{
    /// <summary>
    /// The default publisher or the one set in MediatR configuration.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Executes and awaits each notification handler after one another.
    /// Returns when all handlers complete or an exception has been thrown.
    /// In case of an exception, the rest of the handlers are not executed.
    /// </summary>
    Sequential = 1,

    /// <summary>
    /// Executes and awaits each notification handler after one another.
    /// Returns when all handlers complete. It continues on exception(s).
    /// In case of any exception(s), they will be captured in an AggregateException.
    /// </summary>
    SequentialAll = 2,

    /// <summary>
    /// Executes all notification handlers using Task.WhenAll.
    /// It does not create a separate thread explicitly.
    /// </summary>
    WhenAll = 3,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates new scope using IServiceScopeFactory, executes and awaits all handlers sequentially.
    /// In case of exceptions, if registered in DI, they are logged using ILogger<T>.
    /// </summary>
    SequentialAllNoWait = 4,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates new scope using IServiceScopeFactory, executes and awaits all handlers using Task.WhenAll.
    /// In case of exceptions, if registered in DI, they are logged using ILogger<T>.
    /// </summary>
    WhenAllNoWait = 5
}
