using Envelope.Observables;
using Envelope.Threading;
using Envelope.Transactions.Internal;
using System.Collections.Concurrent;

namespace Envelope.Transactions;

public sealed class TransactionCoordinator : ITransactionCoordinator, ITransactionBehaviorObserverConnector, ITransactionObserverConnector, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	private readonly object _lock = new();
	private readonly AsyncLock _asyncLock = new();

	private readonly TransactionController _transactionController;
	private readonly TransactionBehaviorObserverConnector _transactionBehaviorObserverConnector;
	private readonly TransactionObserverConnector _transactionObserverConnector;

	private bool _disposed;
	private bool _commitRaised;
	private bool _rollbackRaised;

	/// <inheritdoc/>
	public Guid TransactionId { get; }
	public ITransactionController TransactionController => _transactionController;
	public IServiceProvider ServiceProvider { get; }

	/// <inheritdoc/>
	public ConcurrentDictionary<string, object> Items { get; }

	public TransactionCoordinator(IServiceProvider serviceProvider, IEnumerable<ITransactionCacheFactoryStore>? factoryStores)
	{
		TransactionId = Guid.NewGuid();
		Items = new ConcurrentDictionary<string, object>();

		ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		_transactionController = new TransactionController(this);
		_transactionBehaviorObserverConnector = new TransactionBehaviorObserverConnector();
		_transactionObserverConnector = new TransactionObserverConnector();

		if (factoryStores?.Any() == true)
		{
			foreach (var config in factoryStores)
				foreach (var kvp in config.Factories)
					_transactionController.AddTransactionCache(kvp.Key, kvp.Value(ServiceProvider));
		}
		else
		{
			//DEBUG ... potetional bug
		}
	}

	/// <inheritdoc/>
	public IConnectHandle ConnectTransactionObserver(ITransactionBehaviorObserver observer)
		=> _transactionBehaviorObserverConnector.ConnectTransactionObserver(observer);

	/// <inheritdoc/>
	public IConnectHandle ConnectObserver(ITransactionObserver observer)
		=> _transactionObserverConnector.ConnectObserver(observer);

	/// <inheritdoc/>
	public bool Commit()
	{
		if (_disposed)
			throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionCoordinator)}.");

		if (_commitRaised)
			throw new InvalidOperationException($"{nameof(TransactionCoordinator)} is already committed. Multiple commit is not allowed.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot commit {nameof(TransactionCoordinator)} that raised rollback.");

		//try
		//{
			lock (_lock)
			{
				if (_disposed)
					throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionCoordinator)}.");

				if (_commitRaised)
					throw new InvalidOperationException($"{nameof(TransactionCoordinator)} is already committed. Multiple commit is not allowed.");

				if (_rollbackRaised)
					throw new InvalidOperationException($"Cannot commit {nameof(TransactionCoordinator)} that raised rollback.");

				_transactionBehaviorObserverConnector.Lock();
				_transactionObserverConnector.Lock();

				_commitRaised = true;

				_transactionObserverConnector.ForEach(x => x.PreCommit(this));

				_transactionBehaviorObserverConnector.ForEach(x => x.Commit(this));

				_transactionObserverConnector.ForEach(x => x.PostCommit(this));
				return true;
			}
		//}
		//catch (Exception ex)
		//{
		//	Rollback(ex);
		//	return false;
		//}
	}

	/// <inheritdoc/>
	public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
	{
		if (_disposed)
			throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionCoordinator)}.");

		if (_commitRaised)
			throw new InvalidOperationException($"{nameof(TransactionCoordinator)} is already committed. Multiple commit is not allowed.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot commit {nameof(TransactionCoordinator)} that raised rollback.");

		//try
		//{
			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				if (_disposed)
					throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionCoordinator)}.");

				if (_commitRaised)
					throw new InvalidOperationException($"{nameof(TransactionCoordinator)} is already committed. Multiple commit is not allowed.");

				if (_rollbackRaised)
					throw new InvalidOperationException($"Cannot commit {nameof(TransactionCoordinator)} that raised rollback.");

				_transactionBehaviorObserverConnector.Lock();
				_transactionObserverConnector.Lock();

				_commitRaised = true;

				await _transactionObserverConnector.ForEachAsync(x => x.PreCommitAsync(this, cancellationToken)).ConfigureAwait(false);

				await _transactionBehaviorObserverConnector.ForEachAsync(x => x.CommitAsync(this, cancellationToken)).ConfigureAwait(false);

				await _transactionObserverConnector.ForEachAsync(x => x.PostCommitAsync(this, cancellationToken)).ConfigureAwait(false);
				return true;
			}
		//}
		//catch (Exception ex)
		//{
		//	await RollbackAsync(ex, cancellationToken).ConfigureAwait(false);
		//	return false;
		//}
	}

	/// <inheritdoc/>
	public bool Rollback(Exception? exception = null)
	{
		if (_disposed)
			throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionCoordinator)}.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot rollback {nameof(TransactionCoordinator)} multiple times.");

		lock (_lock)
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionCoordinator)}.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot rollback {nameof(TransactionCoordinator)} multiple times.");

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
			throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionCoordinator)}.");

		if (_rollbackRaised)
			throw new InvalidOperationException($"Cannot rollback {nameof(TransactionCoordinator)} multiple times.");

		using (await _asyncLock.LockAsync().ConfigureAwait(false))
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionCoordinator)}.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot rollback {nameof(TransactionCoordinator)} multiple times.");

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
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	private async ValueTask DisposeAsyncCoreAsync()
	{
		await _transactionBehaviorObserverConnector.ForEachAsync(x => x.DisposeAsync());
		_transactionBehaviorObserverConnector.DisconnectAll();
		await _transactionObserverConnector.ForEachAsync(x => x.DisposeAsync());
		_transactionObserverConnector.DisconnectAll();
		await TransactionController.DisposeAsync();
	}
#endif

	/// <inheritdoc/>
	private void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
		{
			_transactionBehaviorObserverConnector.ForEach(x => x.Dispose());
			_transactionBehaviorObserverConnector.DisconnectAll();
			_transactionObserverConnector.ForEach(x => x.Dispose());
			_transactionObserverConnector.DisconnectAll();
			TransactionController.Dispose();
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
