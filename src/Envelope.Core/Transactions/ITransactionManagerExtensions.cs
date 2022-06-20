using System.Diagnostics.CodeAnalysis;

namespace Envelope.Transactions;

public static class ITransactionManagerExtensions
{
	public static T GetItem<T>(this ITransactionManager transactionManager, string key)
	{
		if (transactionManager == null)
			throw new ArgumentNullException(nameof(transactionManager));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (transactionManager.Items.TryGetValue(key, out var v))
			return (T)v;

		throw new ArgumentOutOfRangeException(nameof(key), $"Item with {nameof(key)} = {key} was to found.");
	}

	public static T? GetItemIfExists<T>(this ITransactionManager transactionManager, string key)
	{
		if (transactionManager == null)
			throw new ArgumentNullException(nameof(transactionManager));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (transactionManager.Items.TryGetValue(key, out var v))
			return (T)v;

		return default;
	}

	public static bool TryGetItem<T>(this ITransactionManager transactionManager, string key, [NotNullWhen(true)] out T? value)
	{
		if (transactionManager == null)
			throw new ArgumentNullException(nameof(transactionManager));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (transactionManager.Items.TryGetValue(key, out var v))
		{
			value = (T)v;
			return true;
		}

		value = default;
		return false;
	}

	public static TTransactionManager AddItem<TTransactionManager, T>(this TTransactionManager transactionManager, string key, T value, bool force = false)
		where TTransactionManager : ITransactionManager
	{
		if (transactionManager == null)
			throw new ArgumentNullException(nameof(transactionManager));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (value == null)
			throw new ArgumentNullException(nameof(value));

		if (force)
		{
			transactionManager.Items.AddOrUpdate(key, value, (k, v) => value);
		}
		else
		{
			transactionManager.Items.TryAdd(key, value);
		}

		return transactionManager;
	}

	public static TTransactionManager AddUniqueItem<TTransactionManager, T>(this TTransactionManager transactionManager, string key, T value)
		where TTransactionManager : ITransactionManager
	{
		if (transactionManager == null)
			throw new ArgumentNullException(nameof(transactionManager));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (value == null)
			throw new ArgumentNullException(nameof(value));

		var added = transactionManager.Items.TryAdd(key, value);
		if (!added)
			throw new ArgumentException($"Key {key} was already added.", nameof(key));

		return transactionManager;
	}

	public static TTransactionManager RemoveItem<TTransactionManager, T>(this TTransactionManager transactionManager, string key)
		where TTransactionManager : ITransactionManager
	{
		if (transactionManager == null)
			throw new ArgumentNullException(nameof(transactionManager));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		transactionManager.Items.TryRemove(key, out _);
		return transactionManager;
	}
}
