namespace Envelope.Queries;

public interface IQueryModifier<T>
{
	IEnumerable<T> Apply(IEnumerable<T> enumerable);

	IQueryable<T> Apply(IQueryable<T> queryable);
}

public interface IQueryModifier<T, E> : IQueryModifier<T>
{
	IEnumerable<E> Modify(IEnumerable<T> enumerable);

	IQueryable<E> Modify(IQueryable<T> queryable);
}
