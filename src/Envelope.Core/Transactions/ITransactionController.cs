namespace Envelope.Transactions;

public interface ITransactionController : IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	ITransactionCoordinator TransactionCoordinator { get; }

	TransactionResult TransactionResult { get; }

	string? RollbackErrorInfo { get; }

	void ScheduleCommit();

	void ScheduleRollback(string? rollbackErrorInfo = null);

	TTransactionCache GetTransactionCache<TTransactionCache>()
		where TTransactionCache : ITransactionCache;

	TTransactionCache GetTransactionCache<TTransactionCache>(string name)
		where TTransactionCache : ITransactionCache;

}
