namespace MediatR;

public enum PublishStrategy
{
    /// <summary>
    /// The default publisher or the one set in MediatR configuration.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Executes and awaits each notification handler one after another.
    /// Returns when all handlers complete or an exception has been thrown.
    /// In case of an exception, the rest of the handlers are not executed.
    /// </summary>
    Sequential = 1,

    /// <summary>
    /// Executes and awaits each notification handler one after another.
    /// Returns when all handlers complete. It continues on exception(s).
    /// In case of any exception(s), they will be captured in an AggregateException.
    /// </summary>
    SequentialAll = 2,

    /// <summary>
    /// Executes and awaits all notification handlers using Task.WhenAll.
    /// It does not create a separate thread explicitly.
    /// In case of any exception(s), they will be flattened and captured in an AggregateException.
    /// The AggregateException will contain all exceptions thrown by all handlers, including OperationCanceled exceptions.
    /// </summary>
    WhenAll = 3,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers sequentially.
    /// In case of an exception, it stops further execution. The exception is logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    SequentialNoWait = 11,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers sequentially.
    /// In case of exceptions, they are logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    SequentialAllNoWait = 12,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers using Task.WhenAll.
    /// In case of exceptions, they are logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    WhenAllNoWait = 13
}
