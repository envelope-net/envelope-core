namespace Envelope.Transactions;

public interface ITransactionContext : IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	ITransactionManager TransactionManager { get; }

	TransactionResult TransactionResult { get; }

	string? RollbackErrorInfo { get; }

	void ScheduleCommit();

	void ScheduleRollback(string? rollbackErrorInfo = null);

}
