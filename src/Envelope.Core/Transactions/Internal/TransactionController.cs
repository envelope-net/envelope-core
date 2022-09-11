using System.Collections.Concurrent;

namespace Envelope.Transactions.Internal;

internal class TransactionController : ITransactionController
{
	private bool _disposed;
	private readonly object _lock = new();

	private readonly ConcurrentDictionary<string, ITransactionCache> _transactionCaches;
	private readonly TransactionCoordinator _transactionCoordinator;

	public ITransactionCoordinator TransactionCoordinator => _transactionCoordinator;

	public TransactionResult TransactionResult { get; private set; }

	public string? RollbackErrorInfo { get; private set; }

	public TransactionController(TransactionCoordinator transactionCoordinator)
	{
		_transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
		TransactionResult = TransactionResult.None;
		_transactionCaches = new ConcurrentDictionary<string, ITransactionCache>();
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

	internal void AddTransactionCache<TTransactionCache>(IServiceProvider? serviceProvider = null)
		where TTransactionCache : ITransactionCache
		=> AddTransactionCache<TTransactionCache>(typeof(TTransactionCache)?.FullName!, serviceProvider);

	internal void AddTransactionCache<TTransactionCache>(string name, IServiceProvider? serviceProvider = null)
		where TTransactionCache : ITransactionCache
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name));

		var cache = _transactionCaches.GetOrAdd(name, key =>
		{
			var sp = serviceProvider ?? _transactionCoordinator.ServiceProvider;
			var transactionCache = sp.GetService(typeof(TTransactionCache));
			if (transactionCache == null)
				throw new InvalidOperationException($"{nameof(transactionCache)} == null");

			return (TTransactionCache)transactionCache;
		});

		if (cache is not TTransactionCache transactionCache)
			throw new InvalidOperationException($"Invalid {typeof(ITransactionCache).FullName} type. Returned type = {cache.GetType().FullName} for Name = {name}");

		transactionCache.SetTransactionCoordinatorInternal(TransactionCoordinator);
	}

	internal void AddTransactionCache<TTransactionCache>(TTransactionCache transactionCache)
		where TTransactionCache : ITransactionCache
		=> AddTransactionCache(typeof(TTransactionCache)?.FullName!, transactionCache);

	internal void AddTransactionCache<TTransactionCache>(string name, TTransactionCache transactionCache)
		where TTransactionCache : ITransactionCache
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name));

		if (transactionCache == null)
			throw new ArgumentNullException(nameof(transactionCache));

		var cache = _transactionCaches.GetOrAdd(name, key => transactionCache);
		if (cache is not TTransactionCache transCache)
			throw new InvalidOperationException($"Invalid {typeof(ITransactionCache).FullName} type. Returned type = {cache.GetType().FullName} for Name = {name}");

		transCache.SetTransactionCoordinatorInternal(TransactionCoordinator);
	}

	public TTransactionCache GetTransactionCache<TTransactionCache>()
		where TTransactionCache : ITransactionCache
		=> GetTransactionCache<TTransactionCache>(typeof(TTransactionCache)?.FullName!);

	public TTransactionCache GetTransactionCache<TTransactionCache>(string name)
		where TTransactionCache : ITransactionCache
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name));

		if (!_transactionCaches.TryGetValue(name, out var cache))
			throw new InvalidOperationException($"No cache found for name = {name} | Type = {typeof(ITransactionCache).FullName}");

		if (cache is not TTransactionCache transactionCache)
			throw new InvalidOperationException($"Invalid {typeof(ITransactionCache).FullName} type. Returned type = {cache.GetType().FullName} for name = {name}");

		return transactionCache;
	}

#if NET6_0_OR_GREATER

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual async ValueTask DisposeAsyncCoreAsync()
	{
		foreach (var transItem in _transactionCaches.Values)
			await transItem.DisposeAsync();

		await _transactionCoordinator.DisposeAsync();
		_transactionCaches.Clear();
	}

#endif

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
		{
			foreach (var transItem in _transactionCaches.Values)
				transItem.Dispose();

			_transactionCoordinator.Dispose();
			_transactionCaches.Clear();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
