using Envelope.Expressions;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Envelope.Queries.Sorting;

public class SortDescriptor<T>
{
	private Expression<Func<T, object>>? _memberSelector;
	public Expression<Func<T, object>> MemberSelector
	{
		get => _memberSelector!;
		set
		{
			_memberSelector = value;
			_memberDelegate = null;
		}
	}

	public ListSortDirection SortDirection { get; set; }

	private Func<T, object>? _memberDelegate;
	public Func<T, object> MemberDelegate =>
		_memberDelegate ??= MemberSelector == null
			? throw new InvalidOperationException($"{nameof(MemberSelector)} == null")
			: ExpressionCacheHelper.CompileExpression(MemberSelector);
}
