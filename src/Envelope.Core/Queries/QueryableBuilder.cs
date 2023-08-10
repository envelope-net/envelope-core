using Envelope.Exceptions;
using Envelope.Queries.Includes;
using Envelope.Queries.Paging;
using Envelope.Queries.Sorting;

namespace Envelope.Queries;

public class QueryableBuilder<T> : IQueryableBuilder<T>, IQueryModifier<T>
	where T: class
{
	private readonly List<IQueryModifier<T>> _modifiersStack;

	public QueryableBuilder()
	{
		_modifiersStack = new();
	}

	public QueryableBuilder<T> Sorting(Action<ISortDescriptorBuilder<T>> sorting)
	{
		Throw.ArgumentNull(sorting);

		var builder = new SortDescriptorBuilder<T>();
		sorting.Invoke(builder);
		_modifiersStack.Add(builder);

		return this;
	}

	public QueryableBuilder<T> Paging(int pageIndex, int pageSize)
	{
		var builder = new PagingDescriptorBuilder<T>();
		builder.Page(pageIndex, pageSize);
		_modifiersStack.Add(builder);

		return this;
	}

	public virtual QueryableBuilder<T> Includes(Action<IIncludeBaseDescriptorBuilder<T>> include)
		=> throw new NotImplementedException();

	public QueryableBuilder<T> Modify(IQueryModifier<T> modifier)
	{
		Throw.ArgumentNull(modifier);

		_modifiersStack.Add(modifier);

		return this;
	}

	//public QueryModifierBuilder<T> Modify<E>(IQueryModifier<T, E> modifier)
	//{
	//	Throw.ArgumentNull(modifier);

	//	_modifiersStack.Add(modifier);

	//	return this;
	//}

	IEnumerable<T> IQueryModifier<T>.ApplySort(IEnumerable<T> enumerable)
	{
		Throw.ArgumentNull(enumerable);

		foreach (var modifier in _modifiersStack)
			if (modifier is ISortDescriptorBuilder<T> descriptor)
				enumerable = descriptor.ApplySort(enumerable);

		return enumerable;
	}

	IQueryable<T> IQueryModifier<T>.ApplySort(IQueryable<T> queryable)
	{
		Throw.ArgumentNull(queryable);

		foreach (var modifier in _modifiersStack)
			if (modifier is ISortDescriptorBuilder<T> descriptor)
				queryable = descriptor.ApplySort(queryable);

		return queryable;
	}

	IEnumerable<T> IQueryModifier<T>.ApplyPaging(IEnumerable<T> enumerable)
	{
		Throw.ArgumentNull(enumerable);

		foreach (var modifier in _modifiersStack)
			if (modifier is IPagingDescriptorBuilder<T> descriptor)
				enumerable = descriptor.ApplyPaging(enumerable);

		return enumerable;
	}

	IQueryable<T> IQueryModifier<T>.ApplyPaging(IQueryable<T> queryable)
	{
		Throw.ArgumentNull(queryable);

		foreach (var modifier in _modifiersStack)
			if (modifier is IPagingDescriptorBuilder<T> descriptor)
				queryable = descriptor.ApplyPaging(queryable);

		return queryable;
	}

	IEnumerable<T> IQueryModifier<T>.ApplyIncludes(IEnumerable<T> enumerable)
	{
		Throw.ArgumentNull(enumerable);

		foreach (var modifier in _modifiersStack)
		{
			if (modifier is IIncludeBaseDescriptorBuilder<T> baseDescriptor)
				enumerable = baseDescriptor.ApplyIncludes(enumerable);
			else if (modifier is IIncludeDescriptorBuilder<T> descriptor)
				enumerable = descriptor.ApplyIncludes(enumerable);
			else if (modifier is IThenIncludeDescriptorBuilder<T> thenDescriptor)
				enumerable = thenDescriptor.ApplyIncludes(enumerable);
		}

		return enumerable;
	}

	IQueryable<T> IQueryModifier<T>.ApplyIncludes(IQueryable<T> queryable)
	{
		Throw.ArgumentNull(queryable);

		foreach (var modifier in _modifiersStack)
		{
			if (modifier is IIncludeBaseDescriptorBuilder<T> baseDescriptor)
				queryable = baseDescriptor.ApplyIncludes(queryable);
			else if (modifier is IIncludeDescriptorBuilder<T> descriptor)
				queryable = descriptor.ApplyIncludes(queryable);
			else if (modifier is IThenIncludeDescriptorBuilder<T> thenDescriptor)
				queryable = thenDescriptor.ApplyIncludes(queryable);
		}

		return queryable;
	}

	IEnumerable<T> IQueryModifier<T>.Apply(IEnumerable<T> enumerable)
	{
		Throw.ArgumentNull(enumerable);

		foreach (var modifier in _modifiersStack)
			enumerable = modifier.Apply(enumerable);

		return enumerable;
	}

	IQueryable<T> IQueryModifier<T>.Apply(IQueryable<T> queryable)
	{
		Throw.ArgumentNull(queryable);

		foreach (var modifier in _modifiersStack)
			queryable = modifier.Apply(queryable);

		return queryable;
	}
}
