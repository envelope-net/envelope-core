namespace Envelope.Reflection.ObjectPaths;

public interface IPropertyObjectPath
{
}

public class PropertyObjectPath<T> : ObjectPath<T>, IPropertyObjectPath
{
	internal PropertyObjectPath(Guid id)
		: base()
	{
		Id = id;
	}

	internal PropertyObjectPath(ObjectPath parent, string propertyName)
		: this(Guid.NewGuid())
	{
		if (parent == null)
			throw new ArgumentNullException(nameof(parent));

		if (string.IsNullOrWhiteSpace(propertyName))
			throw new ArgumentNullException(nameof(propertyName));

		PropertyName = propertyName;
		Descendant = null;
		parent.SetDescendant(this, propertyName);
	}

	protected internal override ObjectPath<T> CloneSelf()
		=> new PropertyObjectPath<T>(Id)
		{
			Parent = null,
			PropertyName = PropertyName,
			Descendant = null,
			Depth = Depth,
			Index = null
		};
}
