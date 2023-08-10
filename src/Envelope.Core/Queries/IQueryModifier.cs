namespace Envelope.Queries;

public interface IQueryModifier<T>
{
	IEnumerable<T> Apply(IEnumerable<T> enumerable);

	IQueryable<T> Apply(IQueryable<T> queryable);

	IEnumerable<T> ApplyIncludes(IEnumerable<T> enumerable);

	IQueryable<T> ApplyIncludes(IQueryable<T> queryable);

	IEnumerable<T> ApplySort(IEnumerable<T> enumerable);

	IQueryable<T> ApplySort(IQueryable<T> queryable);

	IEnumerable<T> ApplyPaging(IEnumerable<T> enumerable);

	IQueryable<T> ApplyPaging(IQueryable<T> queryable);
}

public interface IQueryModifier<T, E> : IQueryModifier<T>
{
	IEnumerable<E> Modify(IEnumerable<T> enumerable);

	IQueryable<E> Modify(IQueryable<T> queryable);
}
