namespace Envelope.Transactions;

public interface ITransactionCacheFactoryStore
{
	IReadOnlyDictionary<string, Func<IServiceProvider, ITransactionCache>> Factories { get; }

	ITransactionCacheFactoryStore AddTransactionCache<TTransactionCache>(Func<IServiceProvider, TTransactionCache> factory)
		where TTransactionCache : ITransactionCache;

	ITransactionCacheFactoryStore AddTransactionCache<TTransactionCache>(string name, Func<IServiceProvider, TTransactionCache> factory)
		where TTransactionCache : ITransactionCache;
}
