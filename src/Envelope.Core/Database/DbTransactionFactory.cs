using Envelope.Threading;
using Envelope.Transactions;
using System.Data;
using System.Data.Common;

namespace Envelope.Database;

public class DbTransactionFactory : IDbTransactionFactory, ITransactionCache, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	private readonly DbConnectionFactory? _dbConnectionFactory;
	private readonly DbConnectionFactoryAsync? _dbConnectionFactoryAsync;

	private bool _disposed;
	private bool _initialized;

	private DbConnection? _dbConnection;
	public DbConnection DbConnection => _dbConnection ?? throw new InvalidOperationException("Not initializeed");

	public bool IsInTransaction { get; private set; }

	public DbTransaction? CurrentDbTransaction { get; private set; }

	public ITransactionCoordinator TransactionCoordinator { get; private set; }

	void ITransactionCache.SetTransactionCoordinatorInternal(ITransactionCoordinator transactionCoordinator)
	{
		TransactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
	}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public DbTransactionFactory(DbConnectionFactory dbConnectionFactory)
	{
		if (dbConnectionFactory == null)
			throw new ArgumentNullException(nameof(dbConnectionFactory));

		_dbConnectionFactory = dbConnectionFactory ?? throw new InvalidOperationException($"{nameof(dbConnectionFactory)} returns null");

		CurrentDbTransaction = null;
		IsInTransaction = false;
	}

	public DbTransactionFactory(DbConnectionFactoryAsync dbConnectionFactoryAsync)
	{
		if (dbConnectionFactoryAsync == null)
			throw new ArgumentNullException(nameof(dbConnectionFactoryAsync));

		_dbConnectionFactoryAsync = dbConnectionFactoryAsync ?? throw new InvalidOperationException($"{nameof(dbConnectionFactoryAsync)} returns null");

		CurrentDbTransaction = null;
		IsInTransaction = false;
	}

	public DbTransactionFactory(DbConnection dbConnection, DbTransaction? currentDbTransaction)
	{
		if (dbConnection == null)
			throw new ArgumentNullException(nameof(dbConnection));

		_dbConnectionFactory = new(connectionId => dbConnection);
		CurrentDbTransaction = currentDbTransaction;
		IsInTransaction = CurrentDbTransaction != null;
	}

	public DbTransactionFactory(DbTransaction currentDbTransaction)
	{
		CurrentDbTransaction = currentDbTransaction ?? throw new ArgumentNullException(nameof(currentDbTransaction));

		if (CurrentDbTransaction.Connection == null)
			throw new InvalidOperationException($"{nameof(CurrentDbTransaction.Connection)} == null");

		_dbConnectionFactory = new(connectionId => CurrentDbTransaction.Connection);
		IsInTransaction = CurrentDbTransaction != null;
	}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	private readonly object _initLock = new();
	public void Initialize(string connectionId)
	{
		if (_initialized)
			return;

		lock (_initLock)
		{
			if (_initialized)
				return;

			if (_dbConnectionFactory == null)
				throw new InvalidOperationException($"{nameof(_dbConnectionFactory)} == null");

			_dbConnection = _dbConnectionFactory(connectionId);
			if (_dbConnection == null)
				throw new InvalidOperationException($"{nameof(_dbConnectionFactory)} returns null");

			_initialized = true;
		}
	}

	private readonly AsyncLock _initAsyncLock = new();
	public async Task InitializeAsync(string connectionId)
	{
		if (_initialized)
			return;

		using (await _initAsyncLock.LockAsync())
		{
			if (_initialized)
				return;

			if (_dbConnectionFactoryAsync == null)
			{
				if (_dbConnectionFactory == null)
					throw new InvalidOperationException($"{nameof(_dbConnectionFactoryAsync)} == null AND {nameof(_dbConnectionFactory)} == null");

				_dbConnection = _dbConnectionFactory(connectionId);
				if (_dbConnection == null)
					throw new InvalidOperationException($"{nameof(_dbConnectionFactory)} returns null");
			}
			else
			{
				_dbConnection = await _dbConnectionFactoryAsync(connectionId);
				if (_dbConnection == null)
					throw new InvalidOperationException($"{nameof(_dbConnectionFactoryAsync)} returns null");
			}

			_initialized = true;
		}
	}

	private readonly object _beginTransactionLock = new();
	public DbTransaction BeginTransaction()
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (IsInTransaction || CurrentDbTransaction != null)
			throw new InvalidOperationException($"Transaction already exists");

		lock (_beginTransactionLock)
		{
			if (IsInTransaction || CurrentDbTransaction != null)
				throw new InvalidOperationException($"Transaction already exists");

			CurrentDbTransaction = DbConnection.BeginTransaction();
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}

	public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (IsInTransaction || CurrentDbTransaction != null)
			throw new InvalidOperationException($"Transaction already exists");

		lock (_beginTransactionLock)
		{
			if (IsInTransaction || CurrentDbTransaction != null)
				throw new InvalidOperationException($"Transaction already exists");

			CurrentDbTransaction = DbConnection.BeginTransaction(isolationLevel);
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}

	public DbTransaction GetOrBeginTransaction()
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (CurrentDbTransaction != null)
			return CurrentDbTransaction;

		lock (_beginTransactionLock)
		{
			if (CurrentDbTransaction != null)
				return CurrentDbTransaction;

			CurrentDbTransaction = DbConnection.BeginTransaction();
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}

	public DbTransaction GetOrBeginTransaction(IsolationLevel isolationLevel)
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (CurrentDbTransaction != null)
		{
			if (CurrentDbTransaction.IsolationLevel != isolationLevel)
				throw new InvalidOperationException($"Transaction already exists with another {nameof(isolationLevel)} = {CurrentDbTransaction.IsolationLevel}");

			return CurrentDbTransaction;
		}

		lock (_beginTransactionLock)
		{
			if (CurrentDbTransaction != null)
			{
				if (CurrentDbTransaction.IsolationLevel != isolationLevel)
					throw new InvalidOperationException($"Transaction already exists with another {nameof(isolationLevel)} = {CurrentDbTransaction.IsolationLevel}");

				return CurrentDbTransaction;
			}

			CurrentDbTransaction = DbConnection.BeginTransaction(isolationLevel);
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}

#if NET6_0_OR_GREATER
	private readonly AsyncLock _beginTransactionAsyncLock = new();
	public async ValueTask<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (IsInTransaction || CurrentDbTransaction != null)
			throw new InvalidOperationException($"Transaction already exists");

		using (await _beginTransactionAsyncLock.LockAsync())
		{
			if (IsInTransaction || CurrentDbTransaction != null)
				throw new InvalidOperationException($"Transaction already exists");

			CurrentDbTransaction = await DbConnection.BeginTransactionAsync(cancellationToken);
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}

	public async ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (IsInTransaction || CurrentDbTransaction != null)
			throw new InvalidOperationException($"Transaction already exists");

		using (await _beginTransactionAsyncLock.LockAsync())
		{
			if (IsInTransaction || CurrentDbTransaction != null)
				throw new InvalidOperationException($"Transaction already exists");

			CurrentDbTransaction = await DbConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}

	public async ValueTask<DbTransaction> GetOrBeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (CurrentDbTransaction != null)
			return CurrentDbTransaction;

		using (await _beginTransactionAsyncLock.LockAsync())
		{
			if (CurrentDbTransaction != null)
				return CurrentDbTransaction;

			CurrentDbTransaction = await DbConnection.BeginTransactionAsync(cancellationToken);
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}

	public async ValueTask<DbTransaction> GetOrBeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
	{
		if (!_initialized)
			throw new InvalidOperationException($"Not initialized");

		if (CurrentDbTransaction != null)
		{
			if (CurrentDbTransaction.IsolationLevel != isolationLevel)
				throw new InvalidOperationException($"Transaction already exists with another {nameof(isolationLevel)} = {CurrentDbTransaction.IsolationLevel}");

			return CurrentDbTransaction;
		}

		using (await _beginTransactionAsyncLock.LockAsync())
		{
			if (CurrentDbTransaction != null)
			{
				if (CurrentDbTransaction.IsolationLevel != isolationLevel)
					throw new InvalidOperationException($"Transaction already exists with another {nameof(isolationLevel)} = {CurrentDbTransaction.IsolationLevel}");

				return CurrentDbTransaction;
			}

			CurrentDbTransaction = await DbConnection.BeginTransactionAsync(cancellationToken);
			IsInTransaction = true;
			return CurrentDbTransaction;
		}
	}
#endif

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
		if (CurrentDbTransaction != null)
			await CurrentDbTransaction.DisposeAsync();

		if (_dbConnection != null)
			await _dbConnection.DisposeAsync();
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
			CurrentDbTransaction?.Dispose();

			if (_dbConnection != null)
				_dbConnection.Dispose();
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
