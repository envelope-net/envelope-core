using Envelope.Observables;

namespace Envelope.Transactions;

public interface ITransactionObserverConnector
{
	/// <summary>
	/// Connect an observer
	/// </summary>
	IConnectHandle ConnectObserver(ITransactionObserver observer);
}
