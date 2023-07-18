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
