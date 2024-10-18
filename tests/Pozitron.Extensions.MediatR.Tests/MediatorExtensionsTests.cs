namespace Tests;

public class MediatorExtensionsTests
{
    [Fact]
    public void AddExtendedMediatR_GivenLifetimeAndTypes()
    {
        var services = new ServiceCollection();
        services.AddExtendedMediatR(ServiceLifetime.Scoped, typeof(Ping));

        var mediatorDescriptor = services.First(x => x.ServiceType == typeof(IMediator));
        mediatorDescriptor.ImplementationType.Should().Be<ExtendedMediator>();
        mediatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        services.FirstOrDefault(x => x.ServiceType == typeof(IRequestHandler<Ping>)).Should().NotBeNull();
        services.FirstOrDefault(x => x.ServiceType == typeof(INotificationHandler<Pinged>)).Should().NotBeNull();
    }

    [Fact]
    public void AddExtendedMediatR_GivenTypes()
    {
        var services = new ServiceCollection();
        services.AddExtendedMediatR(typeof(Ping));

        var mediatorDescriptor = services.First(x => x.ServiceType == typeof(IMediator));
        mediatorDescriptor.ImplementationType.Should().Be<ExtendedMediator>();
        mediatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient); // Default one

        services.FirstOrDefault(x => x.ServiceType == typeof(IRequestHandler<Ping>)).Should().NotBeNull();
        services.FirstOrDefault(x => x.ServiceType == typeof(INotificationHandler<Pinged>)).Should().NotBeNull();
    }

    [Fact]
    public void AddExtendedMediatR_GivenLifetimeAndAssemblies()
    {
        var services = new ServiceCollection();
        services.AddExtendedMediatR(ServiceLifetime.Scoped, typeof(Ping).Assembly);

        var mediatorDescriptor = services.First(x => x.ServiceType == typeof(IMediator));
        mediatorDescriptor.ImplementationType.Should().Be<ExtendedMediator>();
        mediatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        services.FirstOrDefault(x => x.ServiceType == typeof(IRequestHandler<Ping>)).Should().NotBeNull();
        services.FirstOrDefault(x => x.ServiceType == typeof(INotificationHandler<Pinged>)).Should().NotBeNull();
    }

    [Fact]
    public void AddExtendedMediatR_GivenAssemblies()
    {
        var services = new ServiceCollection();
        services.AddExtendedMediatR(typeof(Ping));

        var mediatorDescriptor = services.First(x => x.ServiceType == typeof(IMediator));
        mediatorDescriptor.ImplementationType.Should().Be<ExtendedMediator>();
        mediatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient); // Default one

        services.FirstOrDefault(x => x.ServiceType == typeof(IRequestHandler<Ping>)).Should().NotBeNull();
        services.FirstOrDefault(x => x.ServiceType == typeof(INotificationHandler<Pinged>)).Should().NotBeNull();
    }

    [Fact]
    public void AddExtendedMediatR_GivenLambdaConfiguration()
    {
        var services = new ServiceCollection();
        services.AddExtendedMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Ping>();
            cfg.Lifetime = ServiceLifetime.Scoped;
        });

        var mediatorDescriptor = services.First(x => x.ServiceType == typeof(IMediator));
        mediatorDescriptor.ImplementationType.Should().Be<ExtendedMediator>();
        mediatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        services.FirstOrDefault(x => x.ServiceType == typeof(IRequestHandler<Ping>)).Should().NotBeNull();
        services.FirstOrDefault(x => x.ServiceType == typeof(INotificationHandler<Pinged>)).Should().NotBeNull();
    }

    [Fact]
    public void AddExtendedMediatR_GivenConfiguration()
    {
        var services = new ServiceCollection();
        var cfg = new MediatRServiceConfiguration();
        cfg.RegisterServicesFromAssemblyContaining<Ping>();
        cfg.Lifetime = ServiceLifetime.Scoped;
        services.AddExtendedMediatR(cfg);

        var mediatorDescriptor = services.First(x => x.ServiceType == typeof(IMediator));
        mediatorDescriptor.ImplementationType.Should().Be<ExtendedMediator>();
        mediatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        services.FirstOrDefault(x => x.ServiceType == typeof(IRequestHandler<Ping>)).Should().NotBeNull();
        services.FirstOrDefault(x => x.ServiceType == typeof(INotificationHandler<Pinged>)).Should().NotBeNull();
    }

    public record Ping() : IRequest { }
    public class Pong() : IRequestHandler<Ping>
    {
        public Task Handle(Ping request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
    public record Pinged() : INotification { }
    public class Ponged() : INotificationHandler<Pinged>
    {
        public Task Handle(Pinged notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
