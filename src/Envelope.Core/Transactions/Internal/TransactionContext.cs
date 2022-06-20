namespace Envelope.Transactions.Internal;

internal class TransactionContext : ITransactionContext
{
	private readonly object _lock = new();

	public ITransactionManager TransactionManager { get; }

	public TransactionResult TransactionResult { get; private set; }

	public string? RollbackErrorInfo { get; private set; }

	public TransactionContext(ITransactionManager transactionManager)
	{
		TransactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
		TransactionResult = TransactionResult.None;
	}

	public void ScheduleCommit()
	{
		lock (_lock)
		{
			if (TransactionResult != TransactionResult.Rollback)
				TransactionResult = TransactionResult.Commit;
		}
	}

	public void ScheduleRollback(string? rollbackErrorInfo = null)
	{
		lock (_lock)
		{
			TransactionResult = TransactionResult.Rollback;
			RollbackErrorInfo = rollbackErrorInfo;
		}
	}

	public void Dispose()
		=> TransactionManager.Dispose();

#if NET6_0_OR_GREATER
	public ValueTask DisposeAsync()
		=> TransactionManager.DisposeAsync();
#endif
}
