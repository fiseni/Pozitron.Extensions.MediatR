using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests;

public class SequentialTests
{
    private readonly TestQueue<int> _queue = new();

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
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.Sequential);
        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 500);

        var result = _queue.GetValues();
        result.Should().HaveCount(3);
        result[0].Should().Be(Pong1.Id);
        result[1].Should().Be(Pong3.Id);
        result[2].Should().Be(0);
    }

    [Fact]
    public async Task StopsOnException()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(typeof(Ping));
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.Sequential);
        await sut.Should().ThrowAsync<NotImplementedException>();

        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 2, timeoutInMilliseconds: 500);

        var result = _queue.GetValues();
        result.Should().HaveCount(2);
        result[0].Should().Be(Pong1.Id);
        result[1].Should().Be(0);
    }
}
