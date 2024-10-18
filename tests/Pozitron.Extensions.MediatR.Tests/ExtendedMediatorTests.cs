namespace Tests;

// Testing just the edge cases. The rest is covered by the publisher tests.
public class ExtendedMediatorTests
{
    private readonly TestQueue<int> _queue = new();

    [Fact]
    public async Task PublishExtension_ThrowsNotSupported_GivenExtendedMediatorNotRegistered()
    {
        var services = new ServiceCollection();
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.Sequential);
        await sut.Should().ThrowExactlyAsync<NotSupportedException>();
    }

    [Fact]
    public async Task Publish_FallsBackToDefault_GivenInvalidStrategy()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(typeof(Ping));
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), (PublishStrategy)100);
        _queue.Write(0);

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
            await Task.Delay(20, cancellationToken);
            queue.Write(Id);
        }
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
}
