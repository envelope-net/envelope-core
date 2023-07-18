using Envelope.Collections;
using Envelope.Exceptions;
using Envelope.Queries;
using Envelope.Queries.Paging;
using Envelope.Queries.Sorting;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Envelope.Extensions;

public static class IEnumerableExtensions
{
	[return: NotNullIfNotNull("list")]
	public static string? ConvertToString(this IEnumerable list, string delimiter = ", ")
	{
		if (list == null)
			return null;

		var stringBuilder = new StringBuilder();
		bool needDelimiter = false;
		foreach (object current in list)
		{
			if (needDelimiter)
				stringBuilder.Append(delimiter);

			if (current != null)
			{
				stringBuilder.Append(current.ToString());
			}
			else
			{
				stringBuilder.Append("null");
			}
			needDelimiter = true;

		}
		return stringBuilder.ToString();
	}

	public static bool IsEqualsTo<T>(this IEnumerable<T> list, IEnumerable<T> enumerable, IEqualityComparer<T>? comparer = null)
	{
		if (list == null && enumerable == null)
			return true;
		else if (list == null || enumerable == null)
			return false;
		else if (list.Count() != enumerable.Count())
			return false;
		else
		{
			return comparer == null
				? list.SequenceEqual(enumerable)
				: list.SequenceEqual(enumerable, comparer);
		}
	}

	public static T? Get<T>(this IEnumerable<T> list, int index) where T : class
	{
		if (list == null || index < 0 || list.Count() < index)
			return default;

		try
		{
			return list.ElementAtOrDefault(index);
		}
		catch
		{
			return default;
		}
	}

	public static T? Get<T>(this IEnumerable<T> list, int index, T defaultValue)
	{
		if (list == null || index < 0 || list.Count() < index)
			return defaultValue;

		try
		{
			return list.ElementAtOrDefault(index);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
	{
		var col = new ObservableCollection<T>();
		foreach (var cur in enumerable)
			col.Add(cur);

		return col;
	}

	public static string ConvertToString<T>(this IEnumerable<T> source, Func<T, string> selector, string delimiter = ",")
	{
		var b = new StringBuilder();
		bool needDelimiter = false;

		foreach (var item in source)
		{
			if (needDelimiter)
				b.Append(delimiter);

			b.Append(selector(item));
			needDelimiter = true;
		}

		return b.ToString();
	}

	public static IOrderedEnumerable<TSource> OrderBySafe<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector)
	{
		if (enumerable == null)
			throw new ArgumentNullException(nameof(enumerable));

		if (keySelector == null)
			return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

		return enumerable.OrderBy(keySelector);
	}

	public static IOrderedEnumerable<TSource> OrderByDescendingSafe<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector)
	{
		if (enumerable == null)
			throw new ArgumentNullException(nameof(enumerable));

		if (keySelector == null)
			return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

		return enumerable.OrderByDescending(keySelector);
	}

	public static IOrderedEnumerable<TSource> OrderBySafe<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
	{
		if (enumerable == null)
			throw new ArgumentNullException(nameof(enumerable));

		if (propertyName == null)
			return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

		return enumerable.OrderBy(propertyName);
	}

	public static IOrderedEnumerable<TSource> OrderByDescendingSafe<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
	{
		if (enumerable == null)
			throw new ArgumentNullException(nameof(enumerable));

		if (propertyName == null)
			return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

		return enumerable.OrderByDescending(propertyName);
	}

	public static IOrderedEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
	{
		if (string.IsNullOrWhiteSpace(propertyName)) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);
		var queryableSource = enumerable.AsQueryable();
		return new IOrderedEnumerableNoOrderWrapper<TSource>(queryableSource.OrderBy(propertyName));
	}

	public static IOrderedEnumerable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
	{
		if (string.IsNullOrWhiteSpace(propertyName)) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);
		var queryableSource = enumerable.AsQueryable();
		return new IOrderedEnumerableNoOrderWrapper<TSource>(queryableSource.OrderByDescending(propertyName));
	}

	[return: NotNullIfNotNull("enumerable")]
	public static IEnumerable<TSource>? SkipSafe<TSource>(this IEnumerable<TSource> enumerable, int count)
	{
		if (enumerable == null || count < 1)
			return enumerable;

		return enumerable.Skip(count);
	}

	[return: NotNullIfNotNull("enumerable")]
	public static IEnumerable<TSource>? TakeSafe<TSource>(this IEnumerable<TSource> enumerable, int count)
	{
		if (enumerable == null || count < 1)
			return enumerable;

		return enumerable.Take(count);
	}

	/// <summary>
	/// Prevedie IEnumerable&lt;T&gt; na DataTable
	/// </summary>
	/// <param name="data"></param>
	/// <param name="tableName">Nazov dataTable</param>
	/// <param name="columnDisplayTexts">Dictionary&lt;propertyNme, displayNamegt;</param>
	/// <returns></returns>
	public static DataTable? ToDataTable<T>(this IEnumerable<T> data, string? tableName = null, Dictionary<string, string>? columnDisplayTexts = null)
	{
		if (data == null || !data.Any())
			return null;

		var tType = data.GetType().GetGenericArguments()[0];
		var properties = TypeDescriptor.GetProperties(tType);
		if (properties == null) return null;
		string tabName = !string.IsNullOrWhiteSpace(tableName) ? tableName! : tType.ToFriendlyName();
		var table = new DataTable(tabName);

		if (columnDisplayTexts == null || columnDisplayTexts.Count == 0)
		{
			foreach (PropertyDescriptor prop in properties)
			{
				table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
			}

			foreach (T item in data)
			{
				DataRow row = table.NewRow();
				foreach (PropertyDescriptor prop in properties)
				{
					row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
				}
				table.Rows.Add(row);
			}
		}
		else
		{
			foreach (KeyValuePair<string, string> visibleColumn in columnDisplayTexts)
			{
				var prop = properties.Find(visibleColumn.Key, false);
				if (prop != null)
				{
					if (string.IsNullOrWhiteSpace(visibleColumn.Value))
					{
						table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
					}
					else
					{
						table.Columns.Add(visibleColumn.Value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
					}
				}
			}

			foreach (T item in data)
			{
				DataRow row = table.NewRow();
				foreach (KeyValuePair<string, string> visibleColumn in columnDisplayTexts)
				{
					var prop = properties.Find(visibleColumn.Key, false);
					if (prop != null)
					{
						if (string.IsNullOrWhiteSpace(visibleColumn.Value))
						{
							row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
						}
						else
						{
							row[visibleColumn.Value] = prop.GetValue(item) ?? DBNull.Value;
						}
					}
				}
				table.Rows.Add(row);
			}
		}
		return table;
	}

	public static bool HasDuplicates<T>(this IEnumerable<T> source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		var checkBuffer = new HashSet<T>();
		foreach (var t in source)
		{
			if (checkBuffer.Add(t))
				continue;

			return true;
		}

		return false;
	}

	public static bool HasDuplicates<T>(this IEnumerable<T> source, out T? firstDuplicate)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		var checkBuffer = new HashSet<T>();
		foreach (var t in source)
		{
			if (checkBuffer.Add(t))
				continue;

			firstDuplicate = t;
			return true;
		}

		firstDuplicate = default;
		return false;
	}

	public static List<T> GetDuplicates<T>(this IEnumerable<T> source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		var result = new List<T>();
		var checkBuffer = new HashSet<T>();
		foreach (var t in source)
		{
			if (checkBuffer.Add(t))
				continue;

			result.Add(t);
		}

		return result;
	}

	public static List<T> GetDuplicates<T, TCompare>(this IEnumerable<T> source, Func<T?, TCompare?> equalityTransformation)
	{
		if (equalityTransformation == null)
			return GetDuplicates(source);

		if (source == null)
			throw new ArgumentNullException(nameof(source));

		var result = new List<T>();
		var checkBuffer = new HashSet<TCompare?>();
		foreach (var t in source)
		{
			if (checkBuffer.Add(equalityTransformation(t)))
				continue;

			result.Add(t);
		}

		return result;
	}

	public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> values)
		=> ArrayHelper.Permutations(values.ToArray());

	public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, Action<SortDescriptorBuilder<T>>? sorting)
	{
		Throw.ArgumentNull(source);

		if (sorting == null)
			return source;

		var builder = new SortDescriptorBuilder<T>();
		sorting.Invoke(builder);
		return ((IQueryModifier<T>)builder).Apply(source);
	}

	public static IEnumerable<T> GetPage<T>(this IEnumerable<T> source, Action<PagingDescriptorBuilder<T>>? paging)
	{
		Throw.ArgumentNull(source);

		if (paging == null)
			return source;

		var builder = new PagingDescriptorBuilder<T>();
		paging.Invoke(builder);

		return ((IQueryModifier<T>)builder).Apply(source);
	}

	public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, Action<QueryableBuilder<T>>? queryableBuilder)
		where T : class
	{
		Throw.ArgumentNull(source);

		if (queryableBuilder == null)
			return source;

		var builder = new QueryableBuilder<T>();
		queryableBuilder.Invoke(builder);

		return ((IQueryModifier<T>)builder).Apply(source);
	}

	public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, IQueryableBuilder<T>? queryableBuilder)
		where T : class
	{
		Throw.ArgumentNull(source);

		if (queryableBuilder == null)
			return source;

		return queryableBuilder.Apply(source);
	}
}

public class IOrderedEnumerableNoOrderWrapper<T> : IOrderedEnumerable<T>
{
	private readonly IEnumerable<T> source;

	public IOrderedEnumerableNoOrderWrapper(IEnumerable<T> source)
	{
		this.source = source;
	}

	public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey>? comparer, bool descending)
		=> new IOrderedEnumerableNoOrderWrapper<T>(source);

	public IEnumerator<T> GetEnumerator()
	{
		return source.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return source.GetEnumerator();
	}
}
