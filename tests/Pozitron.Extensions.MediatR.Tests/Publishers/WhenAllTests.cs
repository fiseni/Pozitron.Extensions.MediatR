namespace Tests;

public class WhenAllTests
{
    private readonly TestQueue<int> _queue = new();

    [Fact]
    public async Task ExecutesAllHandlersConcurrently()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is (nameof(Pong1) or nameof(Pong7));
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        _queue.Write(0);

        var result = await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 300);
        result.Should().BeEquivalentTo([Pong1.Id, Pong7.Id, 0]);
        result[2].Should().Be(0);
    }

    [Fact]
    public async Task FlattensAllExceptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(typeof(Ping));
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();
        _queue.Write(0);

        var result = await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 300);
        result.Should().BeEquivalentTo([Pong1.Id, Pong7.Id, 0]);
        result[2].Should().Be(0);
        ex.Which.InnerExceptions.Should().HaveCount(8);
    }

    [Fact]
    public async Task ImmediateSingleException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is nameof(Pong2);
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();

        ex.Which.InnerExceptions.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImmediateAggregateException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is nameof(Pong3);
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();

        ex.Which.InnerExceptions.Should().HaveCount(2);
    }

    [Fact]
    public async Task SingleException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is nameof(Pong4);
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();

        ex.Which.InnerExceptions.Should().HaveCount(1);
    }

    [Fact]
    public async Task SingleAggregateException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is nameof(Pong5);
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();

        ex.Which.InnerExceptions.Should().HaveCount(3);
    }

    [Fact]
    public async Task TaskCanceledExceptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is nameof(Pong6);
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();

        ex.Which.InnerExceptions.Should().HaveCount(1);
    }

    public record Ping() : INotification { }

    public class Pong1(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 1;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
    public class Pong2() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new Exception();
    }
    public class Pong3() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new AggregateException(new Exception(), new Exception());
    }
    public class Pong4() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw new Exception();
        }
    }
    public class Pong5() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw new AggregateException(
                new Exception(),
                new AggregateException(new Exception(), new Exception()));
        }
    }
    public class Pong6() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw new TaskCanceledException();
        }
    }

    public class Pong7(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 7;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
}
