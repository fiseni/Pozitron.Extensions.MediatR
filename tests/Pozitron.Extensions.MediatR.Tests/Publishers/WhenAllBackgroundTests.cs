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

        var result = await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 300);
        result.Should().Equal(0, Pong5.Id, Pong1.Id);
    }

    [Fact]
    public async Task FlattensAllExceptions()
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

        var result = await _queue.WaitForCompletion(expectedMessages: 8, timeoutInMilliseconds: 300);
        result.Should().Equal(0, Pong5.Id, Pong1.Id, 21, 31, 32, 33, 41);
        logger.AggregateException.Should().NotBeNull();
        logger.AggregateException!.InnerExceptions.Should().HaveCount(5);
    }

    public record Ping() : INotification { }

    public class Pong1(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 1;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(20, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
    public class Pong2() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new Exception("21");
    }
    public class Pong3() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            throw new AggregateException(
                new Exception("31"),
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
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
}
