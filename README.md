<img align="left" src="pozitronlogo.png" width="120" height="120">

&nbsp; [![NuGet](https://img.shields.io/nuget/v/Pozitron.Extensions.MediatR.svg)](https://www.nuget.org/packages/Pozitron.Extensions.MediatR)

&nbsp; [![Full Build](https://github.com/fiseni/Pozitron.Extensions.MediatR/actions/workflows/build.yml/badge.svg)](https://github.com/fiseni/Pozitron.Extensions.MediatR/actions/workflows/build.yml)

&nbsp; [![Coverage](https://extensionsmediatr.fiseni.com/badge_combined.svg)](https://extensionsmediatr.fiseni.com)

---

A simple library that extends MediatR with various publishing strategies.

## Usage

Set the `MediatorImplementationType` to `ExtendedMediator` in the configuration.

```csharp
services.AddMediatR(x =>
{
    x.MediatorImplementationType = typeof(ExtendedMediator);

    // All your desired configuration.
    x.RegisterServicesFromAssemblyContaining<Ping>();
});
```

Alternatively, use the `AddExtendedMediatR` extension. This extension will set the correct type for you.
```csharp
builder.Services.AddExtendedMediatR(cfg =>
{
    // All your desired configuration.
    cfg.RegisterServicesFromAssemblyContaining<Program>();
});
```

The library provides an additional `Publish` extension to `IMediator`/`IPublisher` with a strategy parameter. You may choose a strategy on the fly.

```csharp
public class Foo(IPublisher publisher)
{
    public async Task Run(CancellationToken cancellationToken)
    {
        // The built-in behavior
        await publisher.Publish(new Ping(), cancellationToken);

        // Publish with specific strategy
        await publisher.Publish(new Ping(), PublishStrategy.WhenAll, cancellationToken);
    }
}
```

## Publish Strategies

Currently, there is support for the following strategies.

```csharp
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
    /// Executes and awaits all notification handlers using Task.WhenAll. It does not create a separate thread explicitly.
    /// In case of any exception(s), they will be flattened and captured in an AggregateException.
    /// The AggregateException will contain all exceptions thrown by all handlers, including OperationCanceled exceptions.
    /// </summary>
    WhenAll = 3,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers sequentially.
    /// In case of an exception, it stops further execution. The exception is logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    SequentialBackground = 11,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers sequentially.
    /// In case of exceptions, they are logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    SequentialAllBackground = 12,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers using Task.WhenAll.
    /// In case of exceptions, they are logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    WhenAllBackground = 13
}
```

## Give a Star! :star:
If you like or are using this project please give it a star. Thanks!
