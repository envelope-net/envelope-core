using System.Collections.Concurrent;

namespace Envelope.Transactions;

public interface ITransactionContext : ITransactionBehaviorObserverConnector, ITransactionObserverConnector, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	/// <summary>
	/// Gets the transaction context identifier.
	/// </summary>
	Guid TransactionId { get; }

	/// <summary>
	/// Custom transaction items.
	/// </summary>
	ConcurrentDictionary<string, object> Items { get; }

	/// <summary>
	/// Executes commit actions enlisted in the transaction with <see cref="ITransactionBehaviorObserverConnector"/>
	/// </summary>
	bool Commit();

	/// <summary>
	/// Executes commit actions enlisted in the transaction with <see cref="ITransactionBehaviorObserverConnector"/>
	/// </summary>
	Task<bool> CommitAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes rollback actions enlisted in the transaction with <see cref="ITransactionBehaviorObserverConnector"/>
	/// </summary>
	bool Rollback(Exception? exception = null);

	/// <summary>
	/// Executes rollback actions enlisted in the transaction with <see cref="ITransactionBehaviorObserverConnector"/>
	/// </summary>
	Task<bool> RollbackAsync(Exception? exception = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes rollback actions enlisted in the transaction with <see cref="ITransactionBehaviorObserverConnector"/>.
	/// Only if the rollback is called for the first time.
	/// </summary>
	bool TryRollback(Exception? exception = null);

	/// <summary>
	/// Executes rollback actions enlisted in the transaction with <see cref="ITransactionBehaviorObserverConnector"/>.
	/// Only if the rollback is called for the first time.
	/// </summary>
	Task<bool> TryRollbackAsync(Exception? exception = null, CancellationToken cancellationToken = default);
}
