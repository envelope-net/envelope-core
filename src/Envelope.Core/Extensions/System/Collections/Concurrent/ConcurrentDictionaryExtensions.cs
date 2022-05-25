using System.Collections.Concurrent;

namespace Envelope.Extensions;

public static class ConcurrentDictionaryExtensions
{
	public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory)
		where TKey : notnull
	{
		if (dictionary == null)
			throw new ArgumentNullException(nameof(dictionary));
		if (addValueFactory == null)
			throw new ArgumentNullException(nameof(addValueFactory));

		return dictionary.AddOrUpdate(
			key,
			addValueFactory,
			(existKey, oldValue) => addValueFactory(key));
	}

	public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue addValue)
		where TKey : notnull
	{
		if (dictionary == null)
			throw new ArgumentNullException(nameof(dictionary));

		return dictionary.AddOrUpdate(
			key,
			addValue,
			(existKey, oldValue) => addValue);
	}
}
