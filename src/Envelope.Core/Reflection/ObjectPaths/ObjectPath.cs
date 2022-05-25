using Envelope.Extensions;
using System.Linq.Expressions;

namespace Envelope.Reflection.ObjectPaths;

public abstract class ObjectPath : IObjectPath
{
	public Guid Id { get; protected set; }
	public Type? ObjectType { get; internal set; }
	public ObjectPath? Parent { get; protected set; }
	public string? PropertyName { get; protected set; }
	public ObjectPath? Descendant { get; protected set; }
	public int Depth { get; protected set; }

	IObjectPath? IObjectPath.Parent => Parent;
	IObjectPath? IObjectPath.Descendant => Descendant;

	internal protected ObjectPath(Type objectType)
	{
		ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
	}

	public static ObjectPathRoot<T> Create<T>()
		=> new();

	internal List<ObjectPath> GetParentsBottomUpPath()
		=> GetParentsPath(new List<ObjectPath>());

	private List<ObjectPath> GetParentsPath(List<ObjectPath> path)
	{
		if (Parent == null)
			return path;

		path.Add(Parent);
		return Parent.GetParentsPath(path);
	}

	public IObjectPath GetRoot()
		=> GetRootInternal();

	private ObjectPath GetRootInternal()
		=> Parent != null
			? Parent.GetRootInternal()
			: this;

	public IObjectPath GetLastDescendant()
		=> GetLastDescendantInternal();

	private ObjectPath GetLastDescendantInternal()
		=> Descendant != null
			? Descendant.GetLastDescendantInternal()
			: this;

	public List<IObjectPath> GetObjectPath()
	{
		var path = new List<IObjectPath>();
		GetObjectPathInternal(path);
		path.Reverse();
		return path;
	}

	private void GetObjectPathInternal(List<IObjectPath> path)
	{
		path.Add(this);
		if (Parent != null)
			Parent.GetObjectPathInternal(path);
	}

	public void SetDescendant(IObjectPath descendant, string propertyName, bool force = true)
	{
		if (descendant == null)
			throw new ArgumentNullException(nameof(descendant));

		if (descendant is not ObjectPath descendantObjectPath)
			throw new ArgumentException($"{nameof(descendant)} must by type of {typeof(ObjectPath).FullName}", nameof(descendant));

		if (string.IsNullOrWhiteSpace(propertyName))
			throw new ArgumentNullException(nameof(propertyName));

		if (this is IPropertyObjectPath)
			throw new NotSupportedException($"{nameof(IPropertyObjectPath)} cannot have {nameof(Descendant)}");

		if (force || Descendant == null)
		{
			descendantObjectPath.PropertyName = propertyName;
			descendantObjectPath.IncreaseDepth(Depth + 1, new HashSet<Guid>());
			descendantObjectPath.Parent = this;
			Descendant = descendantObjectPath;
		}
	}

	private void IncreaseDepth(int delta, HashSet<Guid> usedGuids)
	{
		if (!usedGuids.Add(Id))
			return;

		Depth += delta;

		if (Descendant == null)
			return;

		Descendant.IncreaseDepth(delta, usedGuids);
	}

	protected internal abstract ObjectPath CloneBase();

	public IObjectPath Clone(ObjectPathCloneMode mode)
	{
		var selfClone = CloneBase();

		if (mode == ObjectPathCloneMode.BottomUp && Parent != null)
		{
			var clonedParent = Parent.Clone(ObjectPathCloneMode.BottomUp);
			if (clonedParent != null && clonedParent is ObjectPath clonedParentObjectPath)
				clonedParentObjectPath.SetDescendant(selfClone, selfClone.PropertyName!);
			else
				throw new InvalidOperationException($"Cannot clone {this.GetType().FullName}");
		}

		return selfClone;
	}

	public PropertyObjectPath<TProperty> AddProperty<T, TProperty>(Expression<Func<T, TProperty>> expression)
	{
		if (expression == null)
			throw new ArgumentNullException(nameof(expression));

		var propertyName = expression.GetMemberName() ?? throw new ArgumentException($"propertyName == null", nameof(expression));

		var propertyObjectPath = new PropertyObjectPath<TProperty>(this, propertyName);
		return propertyObjectPath;
	}

	public NavigationObjectPath<TNavigation> AddNavigation<T, TNavigation>(Expression<Func<T, TNavigation>> expression)
	{
		if (expression == null)
			throw new ArgumentNullException(nameof(expression));

		var navigationName = expression.GetMemberName() ?? throw new ArgumentException($"navigationName == null", nameof(expression));

		var navigationObjectPath = new NavigationObjectPath<TNavigation>(this, navigationName);
		return navigationObjectPath;
	}

	public EnumerableObjectPath<TItem> AddEnumerable<T, TItem>(Expression<Func<T, IEnumerable<TItem>>> expression)
	{
		if (expression == null)
			throw new ArgumentNullException(nameof(expression));

		var enumerableName = expression.GetMemberName() ?? throw new ArgumentException($"enumerableName == null", nameof(expression));

		var enumerableObjectPath = new EnumerableObjectPath<TItem>(this, enumerableName);
		return enumerableObjectPath;
	}

	public override string ToString()
		=> string.Join(".", GetObjectPath().Select(x => x.PropertyName ?? "_"));
}
