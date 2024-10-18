namespace Tests;

public class SequentialAllTests
{
    private readonly TestQueue<int> _queue = new();

    [Fact]
    public async Task ExecutesAllHandlersSequentially()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is (nameof(Pong1) or nameof(Pong3));
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.SequentialAll);
        _queue.Write(0);

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, Pong3.Id, 0);
    }

    [Fact]
    public async Task ContinuesOnException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(typeof(Ping));
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.SequentialAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();
        _queue.Write(0);

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, Pong3.Id, 0);
        ex.Which.InnerExceptions.Should().HaveCount(3);
    }

    [Fact]
    public async Task FlattensAggregateException()
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

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.SequentialAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();

        ex.Which.InnerExceptions.Should().HaveCount(2);
    }

    public record Ping() : INotification { }

    public class Pong1(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 1;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            // Adding a delay at the start compared to Pong3. We still expect this to be the first message.
            await Task.Delay(30, cancellationToken);
            queue.Write(Id);
        }
    }
    public class Pong2() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new Exception();
    }
    public class Pong3(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 3;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            queue.Write(Id);
        }
    }
    public class Pong4() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw new AggregateException(new Exception(), new Exception());
        }
    }
}
