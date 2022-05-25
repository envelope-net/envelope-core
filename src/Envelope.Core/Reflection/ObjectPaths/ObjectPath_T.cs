using System.Linq.Expressions;

#nullable disable

namespace Envelope.Reflection.ObjectPaths;

public abstract class ObjectPath<T> : ObjectPath, IObjectPath<T>, IObjectPath
{
	internal protected ObjectPath()
		: base(typeof(T))
	{
	}

	public static ObjectPathRoot<T> Create()
		=> new();

	//internal void AddPathInternal<TProperty>(Expression<Func<T, TProperty>> expression)
	//{
	//	if (expression == null)
	//		throw new ArgumentNullException(nameof(expression));

	//	var memberInfoPath = expression.GetMemberInfoPath(false) ?? throw new ArgumentException($"propertyName == null", nameof(expression));
	//	if (memberInfoPath.Count == 0)
	//		throw new ArgumentException("No members", nameof(expression));

	//	foreach (var memberInfo in memberInfoPath)
	//	{
	//		var propertyOrFieldType = memberInfo.GetFieldOrPropertyType();

	//		if (propertyOrFieldType.IsSimpleType())
	//		{
	//			var propertyObjectPath = new PropertyObjectPath<TPropertyX>(this, memberInfo.Name);
	//		}
	//		else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyOrFieldType))
	//		{
	//			var navigationObjectPath = new NavigationObjectPath<TNavigation>(this, memberInfo.Name);
	//		}
	//		else
	//		{
	//			var enumerableObjectPath = new EnumerableObjectPath<TItem>(this, memberInfo.Name);
	//		}
	//	}
	//}

	protected internal override ObjectPath CloneBase()
		=> CloneSelf();

	protected internal abstract ObjectPath<T> CloneSelf();

	public new IObjectPath<T> Clone(ObjectPathCloneMode mode)
		=> (IObjectPath<T>)base.Clone(mode);

	public PropertyObjectPath<TProperty> AddProperty<TProperty>(Expression<Func<T, TProperty>> expression)
		=> base.AddProperty(expression);

	public NavigationObjectPath<TNavigation> AddNavigation<TNavigation>(Expression<Func<T, TNavigation>> expression)
		=> base.AddNavigation(expression);

	public EnumerableObjectPath<TItem> AddEnumerable<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression)
		=> base.AddEnumerable(expression);
}
