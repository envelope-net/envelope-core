namespace Envelope.Transactions;

public interface ITransactionObserver : Observables.IObserver
{
	void PreCommit(ITransactionManager transactionManager);
	void PostCommit(ITransactionManager transactionManager);
	void PreRollback(ITransactionManager transactionManager, Exception? exception);
	void PostRollback(ITransactionManager transactionManager);
	void RollbackFault(ITransactionManager transactionManager, Exception exception);

	Task PreCommitAsync(ITransactionManager transactionManager, CancellationToken cancellationToken);
	Task PostCommitAsync(ITransactionManager transactionManager, CancellationToken cancellationToken);
	Task PreRollbackAsync(ITransactionManager transactionManager, Exception? exception, CancellationToken cancellationToken);
	Task PostRollbackAsync(ITransactionManager transactionManager, CancellationToken cancellationToken);
	Task RollbackFaultAsync(ITransactionManager transactionManager, Exception exception, CancellationToken cancellationToken);
}
