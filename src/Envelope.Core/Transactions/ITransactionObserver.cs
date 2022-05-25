namespace Envelope.Transactions;

public interface ITransactionObserver : Observables.IObserver
{
	void PreCommit(ITransactionContext transactionContext);
	void PostCommit(ITransactionContext transactionContext);
	void PreRollback(ITransactionContext transactionContext, Exception? exception);
	void PostRollback(ITransactionContext transactionContext);
	void RollbackFault(ITransactionContext transactionContext, Exception exception);

	Task PreCommitAsync(ITransactionContext transactionContext, CancellationToken cancellationToken);
	Task PostCommitAsync(ITransactionContext transactionContext, CancellationToken cancellationToken);
	Task PreRollbackAsync(ITransactionContext transactionContext, Exception? exception, CancellationToken cancellationToken);
	Task PostRollbackAsync(ITransactionContext transactionContext, CancellationToken cancellationToken);
	Task RollbackFaultAsync(ITransactionContext transactionContext, Exception exception, CancellationToken cancellationToken);
}
