using Envelope.Transactions;

namespace Envelope.Database;

public class DbTransactionFactoryBehaviorObserver : ITransactionBehaviorObserver, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	private bool _disposed;
	private readonly IDbTransactionFactory _dbTransactionFactory;

	public DbTransactionFactoryBehaviorObserver(IDbTransactionFactory dbTransactionFactory)
	{
		_dbTransactionFactory = dbTransactionFactory ?? throw new ArgumentNullException(nameof(dbTransactionFactory));
	}

	public void Commit(ITransactionCoordinator transactionCoordinator)
	{
	}

	public Task CommitAsync(ITransactionCoordinator transactionCoordinator, CancellationToken cancellationToken)
		=> Task.CompletedTask;

	public void Rollback(ITransactionCoordinator transactionCoordinator, Exception? exception)
	{
	}

	public Task RollbackAsync(ITransactionCoordinator transactionCoordinator, Exception? exception, CancellationToken cancellationToken)
		=> Task.CompletedTask;

#if NET6_0_OR_GREATER

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual ValueTask DisposeAsyncCoreAsync()
		=> _dbTransactionFactory.DisposeAsync();

#endif

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
			_dbTransactionFactory.Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
