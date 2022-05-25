using Envelope.Exceptions;
using Envelope.Observables;

namespace Envelope.Transactions.Internal;

internal class TransactionBehaviorObserverConnector : ObservableConnector<ITransactionBehaviorObserver>, ITransactionBehaviorObserverConnector
{
	/// <inheritdoc/>
	public IConnectHandle ConnectTransactionManager(ITransactionBehaviorObserver manager)
	{
		try
		{
			return Connect(manager);
		}
		catch (Exception ex)
		{
			throw new TransactionObserverConnectionException("Transaction is in progress. It is not possible to add any more managers.", ex);
		}
	}
}
