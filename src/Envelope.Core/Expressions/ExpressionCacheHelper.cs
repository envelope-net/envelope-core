using System.Linq.Expressions;

namespace Envelope.Expressions;

public static class ExpressionCacheHelper
{
	private static readonly Dictionary<int, Delegate> Cache = new();

	public static Func<T, object> CompileExpression<T>(Expression<Func<T, object>> expression)
	{
		var key = expression.GetHashCode();

		if (Cache.TryGetValue(key, out var cachedDelegate))
			return (Func<T, object>)cachedDelegate;

		var compiledDelegate = expression.Compile();
		Cache[key] = compiledDelegate;
		return compiledDelegate;
	}
}