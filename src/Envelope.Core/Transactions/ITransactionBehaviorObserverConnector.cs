using Envelope.Observables;

namespace Envelope.Transactions;

public interface ITransactionBehaviorObserverConnector
{
	/// <summary>
	/// Connect an transaction observer
	/// </summary>
	IConnectHandle ConnectTransactionObserver(ITransactionBehaviorObserver observer);
}
