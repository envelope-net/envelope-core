using Envelope.Exceptions;

namespace Envelope.Queries.Paging;

public class PagingDescriptorBuilder<T> : IPagingDescriptorBuilder<T>, IQueryModifier<T>
{
	private int _pageIndex;
	private int _pageSize;

	public PagingDescriptorBuilder<T> Page(int pageIndex, int pageSize)
	{
		_pageIndex = pageIndex;
		_pageSize = pageSize;

		return this;
	}

	IEnumerable<T> IQueryModifier<T>.Apply(IEnumerable<T> enumerable)
	{
		Throw.ArgumentNull(enumerable);

		if (0 < _pageIndex && 0 < _pageSize)
			enumerable = enumerable
				.Skip((_pageIndex - 1) * _pageSize)
				.Take(_pageSize);

		return enumerable;
	}

	IQueryable<T> IQueryModifier<T>.Apply(IQueryable<T> queryable)
	{
		Throw.ArgumentNull(queryable);

		if (0 < _pageIndex && 0 < _pageSize)
			queryable = queryable
				.Skip((_pageIndex - 1) * _pageSize)
				.Take(_pageSize);

		return queryable;
	}

	IPagingDescriptorBuilder<T> IPagingDescriptorBuilder<T>.Page(int pageIndex, int pageSize)
		=> Page(pageIndex, pageSize);
}
