namespace Envelope.Transactions;

public interface ITransactionObserver : Observables.IObserver, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	void PreCommit(ITransactionCoordinator transactionCoordinator);
	void PostCommit(ITransactionCoordinator transactionCoordinator);
	void PreRollback(ITransactionCoordinator transactionCoordinator, Exception? exception);
	void PostRollback(ITransactionCoordinator transactionCoordinator);
	void RollbackFault(ITransactionCoordinator transactionCoordinator, Exception exception);

	Task PreCommitAsync(ITransactionCoordinator transactionCoordinator, CancellationToken cancellationToken);
	Task PostCommitAsync(ITransactionCoordinator transactionCoordinator, CancellationToken cancellationToken);
	Task PreRollbackAsync(ITransactionCoordinator transactionCoordinator, Exception? exception, CancellationToken cancellationToken);
	Task PostRollbackAsync(ITransactionCoordinator transactionCoordinator, CancellationToken cancellationToken);
	Task RollbackFaultAsync(ITransactionCoordinator transactionCoordinator, Exception exception, CancellationToken cancellationToken);
}
