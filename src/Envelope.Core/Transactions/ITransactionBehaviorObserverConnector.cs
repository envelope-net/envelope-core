using Envelope.Observables;

namespace Envelope.Transactions;

public interface ITransactionBehaviorObserverConnector
{
	/// <summary>
	/// Connect an transaction manager
	/// </summary>
	IConnectHandle ConnectTransactionManager(ITransactionBehaviorObserver manager);
}
