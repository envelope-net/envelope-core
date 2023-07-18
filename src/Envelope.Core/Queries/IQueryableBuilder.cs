using Envelope.Queries.Includes;
using Envelope.Queries.Sorting;

namespace Envelope.Queries;

public interface IQueryableBuilder
{
}

public interface IQueryableBuilder<T> : IQueryableBuilder, IQueryModifier<T>
	where T : class
{
	QueryableBuilder<T> Sorting(Action<ISortDescriptorBuilder<T>> sorting);

	QueryableBuilder<T> Paging(int pageIndex, int pageSize);

	QueryableBuilder<T> Includes(Action<IIncludeBaseDescriptorBuilder<T>> include);

	QueryableBuilder<T> Modify(IQueryModifier<T> modifier);

	//QueryModifierBuilder<T> Modify<E>(IQueryModifier<T, E> modifier);
}
