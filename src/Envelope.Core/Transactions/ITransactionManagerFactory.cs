namespace Envelope.Transactions;

public interface ITransactionManagerFactory
{
	ITransactionManager Create();
	ITransactionManager Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure);
}
