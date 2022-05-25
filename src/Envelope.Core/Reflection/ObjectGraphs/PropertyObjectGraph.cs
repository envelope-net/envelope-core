//namespace Envelope.Reflection.ObjectGraphs;

//public interface IPropertyObjectGraph
//{
//}

//public class PropertyObjectGraph<T> : ObjectGraph, IPropertyObjectGraph
//{
//	internal PropertyObjectGraph(Guid id)
//		: base(typeof(T))
//	{
//		Id = id;
//	}

//	internal PropertyObjectGraph(ObjectGraph parent, string propertyName)
//		: this(Guid.NewGuid())
//	{
//		if (parent == null)
//			throw new ArgumentNullException(nameof(parent));

//		if (string.IsNullOrWhiteSpace(propertyName))
//			throw new ArgumentNullException(nameof(propertyName));

//		PropertyName = propertyName;
//		Descendants = null;
//		parent.AddDescendant(this, propertyName);
//	}

//	protected override ObjectGraph CloneSelf()
//		=> new PropertyObjectGraph<T>(Id)
//		{
//			Parent = null,
//			PropertyName = PropertyName,
//			Descendants = null,
//			Depth = Depth,
//		};
//}
