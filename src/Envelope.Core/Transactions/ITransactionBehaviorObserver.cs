namespace Envelope.Transactions;

public interface ITransactionBehaviorObserver : Observables.IObserver, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	void Commit(ITransactionManager transactionManager);

	Task CommitAsync(ITransactionManager transactionManager, CancellationToken cancellationToken);

	void Rollback(ITransactionManager transactionManager, Exception? exception);

	Task RollbackAsync(ITransactionManager transactionManager, Exception? exception, CancellationToken cancellationToken);
}
