using Envelope.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Envelope.Reflection;

public static class ReflectionHelper
{
	public static MemberInfo? GetMemberInfo<T>(Expression<Func<T, object?>> expression)  //where T : class
	{
		if (expression == null)
			return null;

		if (expression is not LambdaExpression lambda || lambda.Body == null)
			return null;

		MemberExpression? memberExpression = null;

		// The Func<TTarget, object> we use returns an object, so first statement can be either 
		// a cast (if the field/property does not return an object) or the direct member access.
		if (lambda.Body.NodeType == ExpressionType.Convert)
		{
			// The cast is an unary expression, where the operand is the 
			// actual member access expression.
			if (lambda.Body is not UnaryExpression uexp)
				return null;
			memberExpression = uexp.Operand as MemberExpression;
		}
		else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
		{
			memberExpression = lambda.Body as MemberExpression;
		}

		if (memberExpression == null || memberExpression.Member == null)
			return null;

		return memberExpression.Member;
	}

	public static MemberExpression? GetMemberExpression<T, E>(Expression<Func<T, E>> expression) //where T : class
	{
		if (expression == null)
			return null;

		if (expression is not LambdaExpression lambda || lambda.Body == null)
			return null;

		MemberExpression? memberExpression = null;

		// The Func<TTarget, object> we use returns an object, so first statement can be either 
		// a cast (if the field/property does not return an object) or the direct member access.
		if (lambda.Body.NodeType == ExpressionType.Convert)
		{
			// The cast is an unary expression, where the operand is the 
			// actual member access expression.
			if (lambda.Body is not UnaryExpression uexp)
				return null;

			memberExpression = uexp.Operand as MemberExpression;
		}
		else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
		{
			memberExpression = lambda.Body as MemberExpression;
		}

		return memberExpression;
	}

	public static MemberInfo? GetMemberInfo<T, E>(Expression<Func<T, E>> expression) //where T : class
		=> GetMemberExpression(expression)?.Member;

	public static string? GetMemberName<T>(Expression<Func<T, object?>> expression) //where T : class
	{
		var memberInfo = GetMemberInfo(expression);
		if (memberInfo == null)
			return null;

		return memberInfo.Name;
	}

	public static string? GetMemberName<T, E>(Expression<Func<T, E>> expression) //where T : class
	{
		var memberInfo = GetMemberInfo(expression);
		if (memberInfo == null)
			return null;

		return memberInfo.Name;
	}

	public static string? GetMemberName<T, E>(Expression<Func<T, E>> expression, bool getFullPropertyPath) //where T : class
	{
		if (getFullPropertyPath)
		{
			var memberExpression = GetMemberExpression(expression);
			if (memberExpression == null)
				return null;

			string property = memberExpression.ToString();
			int idx = property.IndexOf(".");
			if (idx != -1)
#if NETSTANDARD2_0 || NETSTANDARD2_1
				property = property.Substring(idx + 1);
#elif NET6_0_OR_GREATER
				property = property[(idx + 1)..];
#endif

			return property;
		}
		else
		{
			return GetMemberName<T, E>(expression);
		}
	}

	/// <example>
	/// pre x => x.ParentChild2List[0].Child2Name, removeMetohds = false
	/// vrati ParentChild2List, get_Item(0), Child2Name
	/// 
	/// pre x => x.ParentChild2List[0].Child2Name, removeMetohds = true
	/// vrati ParentChild2List, Child2Name
	/// 
	/// pre x => x.ParentChild2Enumerable.First().Child2Name, removeMetohds = false
	/// vrati ParentChild2Enumerable, First(), Child2Name
	/// 
	/// pre x => x.ParentChild2Enumerable.First().Child2Name, removeMetohds = true
	/// vrati ParentChild2Enumerable, Child2Name
	/// </example>
	public static List<string>? GetMemberNamePath<T, E>(Expression<Func<T, E>> expression, bool removeMetohds = false) //where T : class
	{
		var memberExpression = GetMemberExpression(expression);
		if (memberExpression == null)
			return null;

		string property = memberExpression.ToString();
		IEnumerable<string> split = property.Split(new char[] { '.' });
		if (1 < split.Count())
			split = split.Skip(1);

		if (removeMetohds)
			split = split.Where(x => !x.Contains('('));

		return split.ToList();
	}

	public static Type? GetFieldOrPropertyType<T>(Expression<Func<T, object?>> expression) //where T : class
	{
		var memberInfo = GetMemberInfo(expression);
		if (memberInfo == null)
			return null;

		if (memberInfo is PropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyType;
		}

		if (memberInfo is FieldInfo fieldInfo)
		{
			return fieldInfo.FieldType;
		}

		return null;
	}

	public static Type? GetFieldOrPropertyType<T, E>(Expression<Func<T, E>> expression) //where T : class
	{
		var memberInfo = GetMemberInfo(expression);
		if (memberInfo == null)
			return null;

		if (memberInfo is PropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyType;
		}

		if (memberInfo is FieldInfo fieldInfo)
		{
			return fieldInfo.FieldType;
		}

		return null;
	}

	public static MemberInfo? GetFirstMemberInfoWithAttribute<T>(IEnumerable<MemberInfo> infoList, Func<T, bool> attributeMatch, bool inherit = true)
		where T : Attribute
	{
		if (infoList == null || attributeMatch == null)
			return null;

		foreach (var mi in infoList)
		{
			var attributes = mi.GetAttributeList<T>(inherit);
			if (attributes != null)
			{
				foreach (T attribute in attributes)
					if (attributeMatch(attribute))
						return mi;
			}
		}

		return null;
	}

	public static List<MemberInfo> GetAllMemberInfosWithAttribute<T>(IEnumerable<MemberInfo> infoList, Func<T, bool> attributeMatch, bool inherit = true)
		where T : Attribute
	{
		var result = new List<MemberInfo>();

		if (infoList == null || attributeMatch == null)
			return result;

		foreach (var mi in infoList)
		{
			var attributes = mi.GetAttributeList<T>(inherit);
			if (attributes != null)
			{
				foreach (T attribute in attributes)
					if (attributeMatch(attribute))
						result.Add(mi);
			}
		}

		return result;
	}

	/// <remarks>
	/// USAGE:<br />
	/// 	Expression<![CDATA[<Func<A, string>>]]> exp = x => x.RefB.RefC.MyProperty;<br />
	/// 	var result = exp.GetMemberInfoPath();<br />
	/// 	<br />
	/// 	Expression<![CDATA[<Func<A, string>>]]> exp2 = x => x.ListB.First().ListC.First().MyProperty;<br />
	/// 	var result2 = exp2.GetMemberInfoPath();
	/// </remarks>
	public static IReadOnlyList<MemberInfo> GetMemberInfoPath<TSource, TResult>(Expression<Func<TSource, TResult>> expression, bool throwIfNoPropertyOrField = true)
	{
		var visitor = new PropertyAndFieldVisitor(throwIfNoPropertyOrField);
		visitor.Visit(expression.Body);
		visitor.Path.Reverse();
		return visitor.Path;
	}

	private class PropertyAndFieldVisitor : ExpressionVisitor
	{
		private readonly bool _throwIfNoPropertyOrField;
		internal readonly List<MemberInfo> Path = new();

		public PropertyAndFieldVisitor(bool throwIfNoPropertyOrField)
		{
			_throwIfNoPropertyOrField= throwIfNoPropertyOrField;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.Member is not PropertyInfo && node.Member is not FieldInfo)
			{
				if (_throwIfNoPropertyOrField)
					throw new ArgumentException("The path can only contain properties or fields", nameof(node));
			}
			else
				Path.Add(node.Member);

			return base.VisitMember(node);
		}
	}
}
