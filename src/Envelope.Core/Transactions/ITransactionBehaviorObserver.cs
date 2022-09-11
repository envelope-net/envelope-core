namespace Envelope.Transactions;

public interface ITransactionBehaviorObserver : Observables.IObserver, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	void Commit(ITransactionCoordinator transactionCoordinator);

	Task CommitAsync(ITransactionCoordinator transactionCoordinator, CancellationToken cancellationToken);

	void Rollback(ITransactionCoordinator transactionCoordinator, Exception? exception);

	Task RollbackAsync(ITransactionCoordinator transactionCoordinator, Exception? exception, CancellationToken cancellationToken);
}
