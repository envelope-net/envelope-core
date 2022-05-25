//namespace Envelope.Reflection.ObjectGraphs;

//public interface INavigationObjectGraph
//{
//}

//public class NavigationObjectGraph<T> : ObjectGraph, INavigationObjectGraph
//{
//	internal NavigationObjectGraph(Guid id)
//		: base(typeof(T))
//	{
//		Id = id;
//	}

//	internal NavigationObjectGraph(ObjectGraph parent, string propertyName)
//		: this(Guid.NewGuid())
//	{
//		if (parent == null)
//			throw new ArgumentNullException(nameof(parent));

//		if (string.IsNullOrWhiteSpace(propertyName))
//			throw new ArgumentNullException(nameof(propertyName));

//		PropertyName = propertyName;
//		Descendants = new Dictionary<string, ObjectGraph>();
//		parent.AddDescendant(this, propertyName);
//	}

//	protected override ObjectGraph CloneSelf()
//		=> new NavigationObjectGraph<T>(Id)
//		{
//			Parent = null,
//			PropertyName = PropertyName,
//			Descendants = new Dictionary<string, ObjectGraph>(),
//			Depth = Depth,
//		};
//}
