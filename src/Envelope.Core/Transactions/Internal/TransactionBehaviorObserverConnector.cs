﻿using Envelope.Exceptions;
using Envelope.Observables;

namespace Envelope.Transactions.Internal;

internal class TransactionBehaviorObserverConnector : ObservableConnector<ITransactionBehaviorObserver>, ITransactionBehaviorObserverConnector
{
	/// <inheritdoc/>
	public IConnectHandle ConnectTransactionObserver(ITransactionBehaviorObserver observer)
	{
		try
		{
			return Connect(observer);
		}
		catch (Exception ex)
		{
			throw new TransactionObserverConnectionException("Transaction is in progress. It is not possible to add any more observers.", ex);
		}
	}
}
