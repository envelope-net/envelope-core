//#if NETSTANDARD2_0 || NETSTANDARD2_1
//using Envelope.Extensions;
//#endif

//namespace Envelope.Reflection.ObjectGraphs;

//public abstract class ObjectGraph : IObjectGraph
//{
//	public Guid Id { get; protected set; }
//	public Type? ObjectType { get; internal set; }
//	public ObjectGraph? Parent { get; protected set; }
//	public string? PropertyName { get; protected set; }
//	public Dictionary<string, ObjectGraph>? Descendants { get; protected set; }
//	public int Depth { get; protected set; }

//	IObjectGraph? IObjectGraph.Parent => Parent;
//	IReadOnlyDictionary<string, IObjectGraph>? IObjectGraph.Descendants => Descendants?.ToDictionary(x => x.Key, x => (IObjectGraph)x.Value);

//	protected ObjectGraph(Type objectType)
//	{
//		ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
//	}

//	internal List<ObjectGraph> GetParentsBottomUpPath()
//		=> GetParentsPath(new List<ObjectGraph>());

//	private List<ObjectGraph> GetParentsPath(List<ObjectGraph> path)
//	{
//		if (Parent == null)
//			return path;

//		path.Add(Parent);
//		return Parent.GetParentsPath(path);
//	}

//	public IObjectGraph GetRootObjectGraph()
//		=> GetRootObjectGraphInternal();

//	private ObjectGraph GetRootObjectGraphInternal()
//		=> Parent != null
//			? Parent.GetRootObjectGraphInternal()
//			: this;

//	public List<IObjectGraph> GetObjectPath()
//	{
//		var path = new List<IObjectGraph>();
//		GetObjectGraphInternal(path);
//		path.Reverse();
//		return path;
//	}

//	private void GetObjectGraphInternal(List<IObjectGraph> path)
//	{
//		path.Add(this);
//		if (Parent != null)
//			Parent.GetObjectGraphInternal(path);
//	}

//	internal ObjectGraph AddDescendant(ObjectGraph descendant, string propertyName)
//	{
//		if (descendant == null)
//			throw new ArgumentNullException(nameof(descendant));

//		if (string.IsNullOrWhiteSpace(propertyName))
//			throw new ArgumentNullException(nameof(propertyName));

//		if (Descendants == null)
//			throw new InvalidOperationException($"{nameof(Descendants)} == null");

//		if (this is IPropertyObjectGraph)
//			throw new NotSupportedException($"{nameof(IPropertyObjectGraph)} cannot have {nameof(Descendants)}");

//		if (Descendants.ContainsKey(propertyName!))
//			throw new InvalidOperationException($"{nameof(Descendants)} already contains {propertyName}");

//		descendant.PropertyName = propertyName;
//		descendant.IncreaseDepth(Depth + 1, new HashSet<Guid>());
//		Descendants.Add(propertyName!, descendant);
//		descendant.Parent = this;

//		return this;
//	}

//	private void IncreaseDepth(int delta, HashSet<Guid> usedGuids)
//	{
//		if (!usedGuids.Add(Id))
//			return;

//		Depth += delta;

//		if (Descendants == null)
//			return;

//		foreach (var descendant in Descendants.Values)
//			descendant.IncreaseDepth(delta, usedGuids);
//	}

//	protected abstract ObjectGraph CloneSelf();

//	public IObjectGraph Clone(ObjectGraphCloneMode mode)
//	{
//		var selfClone = CloneSelf();

//		if (mode == ObjectGraphCloneMode.BottomUp && Parent != null)
//		{
//			var clonedParent = Parent.Clone(ObjectGraphCloneMode.BottomUp);
//			if (clonedParent != null && clonedParent is ObjectGraph clonedParentObjectGraph)
//				clonedParentObjectGraph.AddDescendant(selfClone, selfClone.PropertyName!);
//			else
//				throw new InvalidOperationException($"Cannot clone {this.GetType().FullName}");
//		}

//		return selfClone;
//	}
//}
