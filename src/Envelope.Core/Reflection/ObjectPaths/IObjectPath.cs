using System.Linq.Expressions;

namespace Envelope.Reflection.ObjectPaths;

public interface IObjectPath
{
	Guid Id { get; }
	Type? ObjectType { get; }
	IObjectPath? Parent { get; }
	string? PropertyName { get; }
	IObjectPath? Descendant { get; }
	int Depth { get; }
	int? Index { get; set; }

	IObjectPath GetRoot();
	IObjectPath GetLastDescendant();
	List<IObjectPath> GetObjectPath();
	IObjectPath Clone(ObjectPathCloneMode mode);

	void SetDescendant(IObjectPath descendant, string propertyName, bool force = true);

	PropertyObjectPath<TProperty> AddProperty<T, TProperty>(Expression<Func<T, TProperty>> expression);

	NavigationObjectPath<TNavigation> AddNavigation<T, TNavigation>(Expression<Func<T, TNavigation>> expression);

	EnumerableObjectPath<TItem> AddEnumerable<T, TItem>(Expression<Func<T, IEnumerable<TItem>>> expression);
}

public interface IObjectPath<T> : IObjectPath
{
	PropertyObjectPath<TProperty> AddProperty<TProperty>(Expression<Func<T, TProperty>> expression);

	NavigationObjectPath<TNavigation> AddNavigation<TNavigation>(Expression<Func<T, TNavigation>> expression);

	EnumerableObjectPath<TItem> AddEnumerable<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression);

	new IObjectPath<T> Clone(ObjectPathCloneMode mode);
}