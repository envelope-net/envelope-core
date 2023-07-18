using System.ComponentModel;
using System.Linq.Expressions;

namespace Envelope.Queries.Sorting;

public interface ISortDescriptorBuilder<T> : IQueryModifier<T>
{
	ISortDescriptorBuilder<T> SortBy(Expression<Func<T, object>> memberSelector, ListSortDirection sortDirection = ListSortDirection.Ascending);
}
