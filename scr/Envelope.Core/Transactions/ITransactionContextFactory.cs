namespace Envelope.Transactions;

public interface ITransactionContextFactory
{
	ITransactionContext Create();
	ITransactionContext Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure);
}
