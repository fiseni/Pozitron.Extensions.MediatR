namespace MediatR;

/// <summary>
/// The strategy to use when publishing a notification.
/// </summary>
public enum PublishStrategy
{
    /// <summary>
    /// The default publisher or the one set in MediatR configuration.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Executes and awaits each notification handler one after another.<br/>
    /// Returns when all handlers complete or an exception has been thrown.<br/>
    /// In case of an exception, the rest of the handlers are not executed.<br/>
    /// </summary>
    Sequential = 1,

    /// <summary>
    /// Executes and awaits each notification handler one after another.<br/>
    /// Returns when all handlers complete. It continues on exception(s).<br/>
    /// In case of any exception(s), they will be flattened and captured in an AggregateException.<br/>
    /// The AggregateException will contain all exceptions thrown by all handlers, including OperationCanceled exceptions.<br/>
    /// </summary>
    SequentialAll = 2,

    /// <summary>
    /// Executes and awaits all notification handlers using Task.WhenAll. It does not create a separate thread explicitly.<br/>
    /// In case of any exception(s), they will be flattened and captured in an AggregateException.<br/>
    /// The AggregateException will contain all exceptions thrown by all handlers, including OperationCanceled exceptions.<br/>
    /// </summary>
    WhenAll = 3,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.<br/>
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers sequentially.<br/>
    /// In case of an exception, it stops further execution. The exception is logged using ILogger&lt;T&gt; (if it's registered in DI).<br/>
    /// </summary>
    SequentialBackground = 11,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.<br/>
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers sequentially.<br/>
    /// In case of exception(s), they are logged using ILogger&lt;T&gt; (if it's registered in DI).<br/>
    /// </summary>
    SequentialAllBackground = 12,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.<br/>
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers using Task.WhenAll.<br/>
    /// In case of exception(s), they are logged using ILogger&lt;T&gt; (if it's registered in DI).<br/>
    /// </summary>
    WhenAllBackground = 13
}
