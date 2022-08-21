using System.Diagnostics.CodeAnalysis;

namespace Envelope.Transactions;

public static class TransactionCoordinatorExtensions
{
	public static T GetItem<T>(this ITransactionCoordinator transactionCoordinator, string key)
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (transactionCoordinator.Items.TryGetValue(key, out var v))
			return (T)v;

		throw new ArgumentOutOfRangeException(nameof(key), $"Item with {nameof(key)} = {key} was to found.");
	}

	public static T? GetItemIfExists<T>(this ITransactionCoordinator transactionCoordinator, string key)
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (transactionCoordinator.Items.TryGetValue(key, out var v))
			return (T)v;

		return default;
	}

	public static bool TryGetItem<T>(this ITransactionCoordinator transactionCoordinator, string key, [NotNullWhen(true)] out T? value)
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (transactionCoordinator.Items.TryGetValue(key, out var v))
		{
			value = (T)v;
			return true;
		}

		value = default;
		return false;
	}

	public static TTransactionCoordinator AddItem<TTransactionCoordinator, T>(this TTransactionCoordinator transactionCoordinator, string key, T value, bool force = false)
		where TTransactionCoordinator : ITransactionCoordinator
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (value == null)
			throw new ArgumentNullException(nameof(value));

		if (force)
		{
			transactionCoordinator.Items.AddOrUpdate(key, value, (k, v) => value);
		}
		else
		{
			transactionCoordinator.Items.TryAdd(key, value);
		}

		return transactionCoordinator;
	}

	public static TTransactionCoordinator AddUniqueItem<TTransactionCoordinator, T>(this TTransactionCoordinator transactionCoordinator, string key, T value)
		where TTransactionCoordinator : ITransactionCoordinator
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (value == null)
			throw new ArgumentNullException(nameof(value));

		var added = transactionCoordinator.Items.TryAdd(key, value);
		if (!added)
			throw new ArgumentException($"Key {key} was already added.", nameof(key));

		return transactionCoordinator;
	}

	public static TTransactionCoordinator RemoveItem<TTransactionCoordinator, T>(this TTransactionCoordinator transactionCoordinator, string key)
		where TTransactionCoordinator : ITransactionCoordinator
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		transactionCoordinator.Items.TryRemove(key, out _);
		return transactionCoordinator;
	}
}
