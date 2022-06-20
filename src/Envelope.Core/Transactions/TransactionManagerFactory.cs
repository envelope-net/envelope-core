namespace Envelope.Transactions;

public class TransactionManagerFactory : ITransactionManagerFactory
{
	public ITransactionManager Create()
		=> new TransactionManager();

	public ITransactionManager Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionManager(configureBehavior, configure);

	public static ITransactionManager CreateTransactionManager()
		=> new TransactionManager();

	public static ITransactionManager CreateTransactionManager(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionManager(configureBehavior, configure);
}
