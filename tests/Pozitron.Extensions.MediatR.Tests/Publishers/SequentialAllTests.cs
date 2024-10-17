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
        await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 500);

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, Pong3.Id, 0);
    }

    [Fact]
    public async Task ContinueOnException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(typeof(Ping));
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.SequentialAll);
        await sut.Should().ThrowAsync<NotImplementedException>();

        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 500);

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, Pong3.Id, 0);
    }

    public record Ping() : INotification { }

    public class Pong1(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 1;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            // Adding a delay at the start compared to Pong3. We still expect this to be the first message.
            await Task.Delay(50, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
    public class Pong2() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
    public class Pong3(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 3;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
}
