using Envelope.Observables;
using Envelope.Threading;
using Envelope.Transactions.Internal;
using System.Collections.Concurrent;

namespace Envelope.Transactions;

public class TransactionManager : ITransactionManager, ITransactionBehaviorObserverConnector, ITransactionObserverConnector, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	private readonly object _lock = new();
	private readonly AsyncLock _asyncLock = new();

	private readonly TransactionBehaviorObserverConnector _transactionBehaviorObserverConnector;
	private readonly TransactionObserverConnector _transactionObserverConnector;

	private bool _disposed;
	private bool _commitRaised;
	private bool _rollbackRaised;

	/// <inheritdoc/>
	public Guid TransactionId { get; }

	/// <inheritdoc/>
	public ConcurrentDictionary<string, object> Items { get; }

	protected internal TransactionManager()
	{
		TransactionId = Guid.NewGuid();
		Items = new ConcurrentDictionary<string, object>();

		_transactionBehaviorObserverConnector = new TransactionBehaviorObserverConnector();
		_transactionObserverConnector = new TransactionObserverConnector();
	}

	protected internal TransactionManager(
		Action<ITransactionBehaviorObserverConnector>? configureBehavior,
		Action<ITransactionObserverConnector>? configure)
		: this()
	{
		configureBehavior?.Invoke(_transactionBehaviorObserverConnector);
		configure?.Invoke(_transactionObserverConnector);
	}

	/// <inheritdoc/>
	public IConnectHandle ConnectTransactionObserver(ITransactionBehaviorObserver manager)
		=> _transactionBehaviorObserverConnector.ConnectTransactionObserver(manager);

	/// <inheritdoc/>
	public IConnectHandle ConnectObserver(ITransactionObserver observer)
		=> _transactionObserverConnector.ConnectObserver(observer);

	/// <inheritdoc/>
	public bool Commit()
	{
		if (_disposed)
			throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionManager)}.");

		if (_commitRaised)
			throw new InvalidOperationException($"{nameof(TransactionManager)} is already committed. Multiple commit is not allowed.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot commit {nameof(TransactionManager)} that raised rollback.");

		lock (_lock)
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionManager)}.");

			if (_commitRaised)
				throw new InvalidOperationException($"{nameof(TransactionManager)} is already committed. Multiple commit is not allowed.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot commit {nameof(TransactionManager)} that raised rollback.");

			_transactionBehaviorObserverConnector.Lock();
			_transactionObserverConnector.Lock();

			_commitRaised = true;

			_transactionObserverConnector.ForEach(x => x.PreCommit(this));

			try
			{
				_transactionBehaviorObserverConnector.ForEach(x => x.Commit(this));
			}
			catch (Exception ex)
			{
				Rollback(ex);
				return false;
			}

			_transactionObserverConnector.ForEach(x => x.PostCommit(this));
			return true;
		}
	}

	/// <inheritdoc/>
	public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
	{
		if (_disposed)
			throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionManager)}.");

		if (_commitRaised)
			throw new InvalidOperationException($"{nameof(TransactionManager)} is already committed. Multiple commit is not allowed.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot commit {nameof(TransactionManager)} that raised rollback.");

		using(await _asyncLock.LockAsync().ConfigureAwait(false))
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionManager)}.");

			if (_commitRaised)
				throw new InvalidOperationException($"{nameof(TransactionManager)} is already committed. Multiple commit is not allowed.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot commit {nameof(TransactionManager)} that raised rollback.");

			_transactionBehaviorObserverConnector.Lock();
			_transactionObserverConnector.Lock();

			_commitRaised = true;

			await _transactionObserverConnector.ForEachAsync(x => x.PreCommitAsync(this, cancellationToken)).ConfigureAwait(false);

			try
			{
				await _transactionBehaviorObserverConnector.ForEachAsync(x => x.CommitAsync(this, cancellationToken)).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await RollbackAsync(ex, cancellationToken).ConfigureAwait(false);
				return false;
			}

			await _transactionObserverConnector.ForEachAsync(x => x.PostCommitAsync(this, cancellationToken)).ConfigureAwait(false);
			return true;
		}
	}

	/// <inheritdoc/>
	public bool Rollback(Exception? exception = null)
	{
		if (_disposed)
			throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionManager)}.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot rollback {nameof(TransactionManager)} multiple times.");

		lock (_lock)
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionManager)}.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot rollback {nameof(TransactionManager)} multiple times.");

			_transactionBehaviorObserverConnector.Lock();
			_transactionObserverConnector.Lock();

			_rollbackRaised = true;

			_transactionObserverConnector.ForEach(x => x.PreRollback(this, exception));

			try
			{
				_transactionBehaviorObserverConnector.ForEach(x => x.Rollback(this, exception));
			}
			catch (Exception ex)
			{
				_transactionObserverConnector.ForEach(x => x.RollbackFault(this, ex));
				return false;
			}

			_transactionObserverConnector.ForEach(x => x.PostRollback(this));
			return true;
		}
	}

	/// <inheritdoc/>
	public async Task<bool> RollbackAsync(Exception? exception = null, CancellationToken cancellationToken = default)
	{
		if (_disposed)
			throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionManager)}.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot rollback {nameof(TransactionManager)} multiple times.");

		using (await _asyncLock.LockAsync().ConfigureAwait(false))
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionManager)}.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot rollback {nameof(TransactionManager)} multiple times.");

			_transactionBehaviorObserverConnector.Lock();
			_transactionObserverConnector.Lock();

			_rollbackRaised = true;

			await _transactionObserverConnector.ForEachAsync(x => x.PreRollbackAsync(this, exception, cancellationToken)).ConfigureAwait(false);

			try
			{
				await _transactionBehaviorObserverConnector.ForEachAsync(x => x.RollbackAsync(this, exception, cancellationToken)).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _transactionObserverConnector.ForEachAsync(x => x.RollbackFaultAsync(this, ex, cancellationToken)).ConfigureAwait(false);
				return false;
			}

			await _transactionObserverConnector.ForEachAsync(x => x.PostRollbackAsync(this, cancellationToken)).ConfigureAwait(false);
			return true;
		}
	}

	/// <inheritdoc/>
	public bool TryRollback(Exception? exception = null)
	{
		if (_disposed || _rollbackRaised)
			return false;

		lock (_lock)
		{
			if (_disposed || _rollbackRaised)
				return false;

			_transactionBehaviorObserverConnector.Lock();
			_transactionObserverConnector.Lock();

			_rollbackRaised = true;

			_transactionObserverConnector.ForEach(x => x.PreRollback(this, exception));

			try
			{
				_transactionBehaviorObserverConnector.ForEach(x => x.Rollback(this, exception));
			}
			catch (Exception ex)
			{
				_transactionObserverConnector.ForEach(x => x.RollbackFault(this, ex));
				return false;
			}

			_transactionObserverConnector.ForEach(x => x.PostRollback(this));
			return true;
		}
	}

	/// <inheritdoc/>
	public async Task<bool> TryRollbackAsync(Exception? exception = null, CancellationToken cancellationToken = default)
	{
		if (_disposed || _rollbackRaised)
			return false;

		using (await _asyncLock.LockAsync().ConfigureAwait(false))
		{
			if (_disposed || _rollbackRaised)
				return false;

			_transactionBehaviorObserverConnector.Lock();
			_transactionObserverConnector.Lock();

			_rollbackRaised = true;

			await _transactionObserverConnector.ForEachAsync(x => x.PreRollbackAsync(this, exception, cancellationToken)).ConfigureAwait(false);

			try
			{
				await _transactionBehaviorObserverConnector.ForEachAsync(x => x.RollbackAsync(this, exception, cancellationToken)).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _transactionObserverConnector.ForEachAsync(x => x.RollbackFaultAsync(this, ex, cancellationToken)).ConfigureAwait(false);
				return false;
			}

			await _transactionObserverConnector.ForEachAsync(x => x.PostRollbackAsync(this, cancellationToken)).ConfigureAwait(false);
			return true;
		}
	}

	public bool TryAddItem<T>(string key, T value)
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (value == null)
			throw new ArgumentNullException(nameof(value));

		var added = Items.TryAdd(key, value);
		return added;
	}

#if NET6_0_OR_GREATER
	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual ValueTask DisposeAsyncCoreAsync()
		=> _transactionBehaviorObserverConnector.ForEachAsync(x => x.DisposeAsync());
#endif

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_transactionBehaviorObserverConnector.ForEach(x => x.Dispose());
			_disposed = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public virtual ITransactionContext CreateTransactionContext()
		=> new TransactionContext(this);
}
