using Envelope.Transactions.Internal;

namespace Envelope.Transactions;

public class TransactionContextFactory : ITransactionContextFactory
{
	public ITransactionContext Create()
		=> new TransactionContext();

	public ITransactionContext Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionContext(configureBehavior, configure);

	public static ITransactionContext CreateTransactionContext()
		=> new TransactionContext();

	public static ITransactionContext CreateTransactionContext(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionContext(configureBehavior, configure);
}
