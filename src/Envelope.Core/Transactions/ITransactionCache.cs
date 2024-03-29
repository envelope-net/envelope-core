﻿namespace Envelope.Transactions;

public interface ITransactionCache : IDisposable
#if NET6_0_OR_GREATER
	, IAsyncDisposable
#endif
{
	ITransactionCoordinator TransactionCoordinator { get; }

	void SetTransactionCoordinatorInternal(ITransactionCoordinator transactionCoordinator);
}
