namespace Envelope.Transactions;

public interface ITransactionBehaviorObserver : Observables.IObserver, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	void Commit(ITransactionContext transactionContext);

	Task CommitAsync(ITransactionContext transactionContext, CancellationToken cancellationToken);

	void Rollback(ITransactionContext transactionContext, Exception? exception);

	Task RollbackAsync(ITransactionContext transactionContext, Exception? exception, CancellationToken cancellationToken);
}
