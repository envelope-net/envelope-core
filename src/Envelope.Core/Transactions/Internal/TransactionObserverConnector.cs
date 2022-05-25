using Envelope.Exceptions;
using Envelope.Observables;

namespace Envelope.Transactions.Internal;

internal class TransactionObserverConnector : ObservableConnector<ITransactionObserver>, ITransactionObserverConnector
{
	/// <inheritdoc/>
	public IConnectHandle ConnectObserver(ITransactionObserver observer)
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
