using System.Linq.Expressions;

namespace Envelope.Reflection.ObjectPaths;

public interface IObjectPathRoot
{
}

public class ObjectPathRoot<T> : ObjectPath<T>, IObjectPathRoot
{
	internal ObjectPathRoot(Guid id)
		: base()
	{
		Id = id;
	}

	internal ObjectPathRoot()
		: this(Guid.NewGuid())
	{
		Parent = null;
		PropertyName = null;
		Descendant = null;
		Depth = 0;
	}

	protected internal override ObjectPath<T> CloneSelf()
		=> new ObjectPathRoot<T>(Id)
		{
			Parent = null,
			PropertyName = null,
			Descendant = null,
			Depth = Depth,
		};
}
