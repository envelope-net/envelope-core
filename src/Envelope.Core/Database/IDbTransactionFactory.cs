using Envelope.Transactions;
using System.Data;
using System.Data.Common;

namespace Envelope.Database;

public interface IDbTransactionFactory : ITransactionCache, IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	DbConnection DbConnection { get; }

	bool IsInTransaction { get; }

	DbTransaction? CurrentDbTransaction { get; }

	void Initialize(string connectionId);

	Task InitializeAsync(string connectionId);

	DbTransaction BeginTransaction();

	DbTransaction BeginTransaction(IsolationLevel isolationLevel);

	DbTransaction GetOrBeginTransaction();

	DbTransaction GetOrBeginTransaction(IsolationLevel isolationLevel);

#if NET6_0_OR_GREATER
	ValueTask<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

	ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

	ValueTask<DbTransaction> GetOrBeginTransactionAsync(CancellationToken cancellationToken = default);

	ValueTask<DbTransaction> GetOrBeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
#endif
}
