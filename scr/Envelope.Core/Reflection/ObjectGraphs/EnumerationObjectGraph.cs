//namespace Envelope.Reflection.ObjectGraphs;

//public interface IEnumerationObjectGraph
//{
//}

////public abstract class EnumerationObjectGraph<T> : NavigationObjectGraph<T>
////{
////	internal EnumerationObjectGraph(Guid id)
////		: base(id)
////	{
////	}

////	internal EnumerationObjectGraph(ObjectGraph parent, string propertyName)
////		: base(parent, propertyName)
////	{
////	}
////}

//public class EnumerationObjectGraph<TEnumerable, T> : NavigationObjectGraph<T> /*EnumerationObjectGraph<T>*/, IEnumerationObjectGraph
//	where TEnumerable : IEnumerable<T>
//{
//	internal EnumerationObjectGraph(Guid id)
//		: base(id)
//	{
//	}

//	internal EnumerationObjectGraph(ObjectGraph parent, string propertyName)
//		: base(parent, propertyName)
//	{
//	}

//	protected override ObjectGraph CloneSelf()
//		=> new EnumerationObjectGraph<TEnumerable, T>(Id)
//		{
//			Parent = null,
//			PropertyName = PropertyName,
//			Descendants = new Dictionary<string, ObjectGraph>(),
//			Depth = Depth,
//		};
//}
