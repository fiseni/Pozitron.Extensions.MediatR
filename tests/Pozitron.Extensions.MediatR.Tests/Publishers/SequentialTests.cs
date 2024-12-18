﻿namespace Tests;

public class SequentialTests
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

        await mediator.Publish(new Ping(), PublishStrategy.Sequential);
        _queue.Write(0);

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, Pong3.Id, 0);
    }

    [Fact]
    public async Task StopsOnException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(typeof(Ping));
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.Sequential);
        await sut.Should().ThrowExactlyAsync<NotImplementedException>();
        _queue.Write(0);

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, 0);
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
            await Task.Yield();
            queue.Write(Id);
        }
    }
}
