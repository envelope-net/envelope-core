using Envelope.Exceptions;
using Envelope.Extensions;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Envelope.Queries.Sorting;

public class SortDescriptorBuilder<T>
{
	private readonly List<SortDescriptor<T>> _sortStack;

	public SortDescriptorBuilder()
	{
		_sortStack = new();
	}

	public SortDescriptorBuilder<T> SortBy(Expression<Func<T, object>>? memberSelector, ListSortDirection sortDirection = ListSortDirection.Ascending)
	{
		Throw.ArgumentNull(memberSelector);

		_sortStack.Add(new SortDescriptor<T>
		{
			MemberSelector = memberSelector,
			SortDirection = sortDirection
		});

		return this;
	}

	internal IEnumerable<T> Apply(IEnumerable<T> enumerable)
	{
		Throw.ArgumentNull(enumerable);

		var first = true;
		IOrderedEnumerable<T>? orderedQueryable = null;
		foreach (var sort in _sortStack)
		{
			if (first)
			{
				if (sort.SortDirection == ListSortDirection.Ascending)
				{
					orderedQueryable = enumerable.OrderBy(sort.MemberDelegate);
				}
				else
				{
					orderedQueryable = enumerable.OrderByDescending(sort.MemberDelegate);
				}
				first = false;
			}
			else
			{
				if (sort.SortDirection == ListSortDirection.Ascending)
				{
					orderedQueryable = orderedQueryable.ThenBy(sort.MemberDelegate);
				}
				else
				{
					orderedQueryable = orderedQueryable.ThenByDescending(sort.MemberDelegate);
				}
			}
		}

		return orderedQueryable ?? enumerable;
	}

	internal IQueryable<T> Apply(IQueryable<T> queryable)
	{
		Throw.ArgumentNull(queryable);

		var first = true;
		IOrderedQueryable<T>? orderedQueryable = null;
		foreach (var sort in _sortStack)
		{
			if (first)
			{
				if (sort.SortDirection == ListSortDirection.Ascending)
				{
					orderedQueryable = queryable.OrderBy(sort.MemberSelector);
				}
				else
				{
					orderedQueryable = queryable.OrderByDescending(sort.MemberSelector);
				}
				first = false;
			}
			else
			{
				if (sort.SortDirection == ListSortDirection.Ascending)
				{
					orderedQueryable = orderedQueryable.ThenBy(sort.MemberSelector);
				}
				else
				{
					orderedQueryable = orderedQueryable.ThenByDescending(sort.MemberSelector);
				}
			}
		}

		return orderedQueryable ?? queryable;
	}
}
