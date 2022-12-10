namespace Envelope.Reflection.ObjectPaths;

public interface IEnumerableObjectPath
{
}

public class EnumerableObjectPath<T> : NavigationObjectPath<T>, IEnumerableObjectPath
{
	internal EnumerableObjectPath(Guid id)
		: base(id)
	{
	}

	internal EnumerableObjectPath(ObjectPath parent, string propertyName)
		: base(parent, propertyName)
	{
	}

	protected internal override ObjectPath<T> CloneSelf()
		=> new EnumerableObjectPath<T>(Id)
		{
			Parent = null,
			PropertyName = PropertyName,
			Descendant = null,
			Depth = Depth,
			Index = null
		};
}
