using Envelope.Extensions;

namespace Envelope.Transactions;

public class TransactionCacheFactoryStore : ITransactionCacheFactoryStore
{
	private readonly Dictionary<string, Func<IServiceProvider, ITransactionCache>> _factories;
	public IReadOnlyDictionary<string, Func<IServiceProvider, ITransactionCache>> Factories => _factories;

	public TransactionCacheFactoryStore()
	{
		_factories = new Dictionary<string, Func<IServiceProvider, ITransactionCache>>();
	}

	public TransactionCacheFactoryStore(string name, Func<IServiceProvider, ITransactionCache> factory)
		: this()
	{
		AddTransactionCache(name, factory);
	}

	public ITransactionCacheFactoryStore AddTransactionCache<TTransactionCache>(Func<IServiceProvider, TTransactionCache> factory)
		where TTransactionCache : ITransactionCache
		=> AddTransactionCache(typeof(TTransactionCache).FullName!, factory);

	public ITransactionCacheFactoryStore AddTransactionCache<TTransactionCache>(string name, Func<IServiceProvider, TTransactionCache> factory)
		where TTransactionCache : ITransactionCache
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name));

		if (factory == null)
			throw new ArgumentNullException(nameof(factory));

		var added = _factories.TryAdd(name, sp => factory(sp));
		if (!added)
			throw new InvalidOperationException($"{nameof(name)} = {name} already added");

		return this;
	}
}
