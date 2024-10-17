namespace Tests;

public class WhenAllBackgroundTests
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
            x.TypeEvaluator = type => type.Name is (nameof(Pong1) or nameof(Pong5));
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.WhenAllBackground);

        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 300);

        var result = _queue.GetValues();
        result.Should().Equal(0, Pong1.Id, Pong5.Id);
    }

    [Fact]
    public async Task FlattensExceptions()
    {
        var logger = new FakeLogger<ExtendedMediator>(_queue);
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddSingleton(typeof(ILogger<ExtendedMediator>), _ => logger);
        services.AddExtendedMediatR(typeof(Ping));
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.WhenAllBackground);

        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 10, timeoutInMilliseconds: 300);

        var result = _queue.GetValues();
        result.Should().Equal(0, Pong1.Id, Pong5.Id, 21, 22, 23, 31, 32, 33, 41);

        logger.Exception.Should().NotBeNull();
        logger.Exception.Should().BeOfType<AggregateException>();
        var aggregateException = (AggregateException)logger.Exception!;
        aggregateException.InnerExceptions.Should().HaveCount(7);
    }

    public record Ping() : INotification { }

    public class Pong1(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 1;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
    public class Pong2() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new AggregateException(
                new NotSupportedException("21"),
                new AggregateException(new Exception("22"), new Exception("23")));
    }
    public class Pong3() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            throw new AggregateException(
                new NotSupportedException("31"),
                new AggregateException(new Exception("32"), new Exception("33")));
        }
    }
    public class Pong4() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(30, cancellationToken);
            throw new TaskCanceledException("41");
        }
    }
    public class Pong5(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 5;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(30, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
}
