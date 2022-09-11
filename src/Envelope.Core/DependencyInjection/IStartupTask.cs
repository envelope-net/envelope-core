namespace Envelope.DependencyInjection;

public interface IStartupTask
{
	Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}
