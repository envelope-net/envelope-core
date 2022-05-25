//namespace Envelope.Reflection.ObjectGraphs;

//public interface IRootObjectGraph
//{
//}

//public class RootObjectGraph<T> : ObjectGraph, IRootObjectGraph
//{
//	internal RootObjectGraph(Guid id)
//		: base(typeof(T))
//	{
//		Id = id;
//	}

//	internal RootObjectGraph()
//		: this(Guid.NewGuid())
//	{
//		Parent = null;
//		PropertyName = null;
//		Descendants = new Dictionary<string, ObjectGraph>();
//		Depth = 0;
//	}

//	protected override ObjectGraph CloneSelf()
//		=> new RootObjectGraph<T>(Id)
//		{
//			Parent = null,
//			PropertyName = null,
//			Descendants = new Dictionary<string, ObjectGraph>(),
//			Depth = Depth,
//		};
//}
