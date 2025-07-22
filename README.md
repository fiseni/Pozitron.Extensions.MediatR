<img align="left" src="pozitronlogo.png" width="120" height="120">

&nbsp; [![NuGet](https://img.shields.io/nuget/v/Pozitron.Extensions.MediatR.svg)](https://www.nuget.org/packages/Pozitron.Extensions.MediatR)

&nbsp; [![Full Build](https://github.com/fiseni/Pozitron.Extensions.MediatR/actions/workflows/build.yml/badge.svg)](https://github.com/fiseni/Pozitron.Extensions.MediatR/actions/workflows/build.yml)

&nbsp; [![Coverage](https://extensionsmediatr.fiseni.com/badge_combined.svg)](https://extensionsmediatr.fiseni.com)

---
# Pozitron.Extensions.MediatR

A simple library that extends MediatR with various publishing strategies. I elaborated on the motivation and implementation details in this [article](https://fiseni.com/posts/mediatr-publishing-strategies/).

> [!IMPORTANT]  
> This extension library is published under the MIT license. Please note that starting with version 13.0.0, the [MediatR](https://github.com/LuckyPennySoftware/MediatR) library (which this project extends) has adopted a dual licensing model. For details about MediatR’s licensing and pricing, visit https://mediatr.io.

## Usage

Set the `MediatorImplementationType` to `ExtendedMediator` in the configuration.

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.MediatorImplementationType = typeof(ExtendedMediator);

    // All your desired configuration.
    cfg.RegisterServicesFromAssemblyContaining<Ping>();
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

The library defines a `Publish` extension that accepts a strategy as a parameter. You may choose a strategy on the fly.

```csharp
public class Foo(IMediator mediator)
{
    public async Task Run(CancellationToken cancellationToken)
    {
        // The built-in behavior
        await mediator.Publish(new Ping(), cancellationToken);

        // Publish with specific strategy
        await mediator.Publish(new Ping(), PublishStrategy.WhenAll, cancellationToken);
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
    /// In case of any exception(s), they will be flattened and captured in an AggregateException.
    /// The AggregateException will contain all exceptions thrown by all handlers, including OperationCanceled exceptions.
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
    /// In case of exception(s), they are logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    SequentialAllBackground = 12,

    /// <summary>
    /// Creates a single new thread using Task.Run(), and returns Task.Completed immediately.
    /// Creates a new scope using IServiceScopeFactory, executes and awaits all handlers using Task.WhenAll.
    /// In case of exception(s), they are logged using ILogger<T> (if it's registered in DI).
    /// </summary>
    WhenAllBackground = 13
}
```

## Give a Star! :star:
If you like or are using this project please give it a star. Thanks!
