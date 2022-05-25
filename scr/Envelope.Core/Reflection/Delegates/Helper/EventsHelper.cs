using System.Reflection;

namespace Envelope.Reflection.Delegates.Helper;

internal static partial class EventsHelper
{
	public static readonly MethodInfo EventHandlerFactoryMethodInfo =
		typeof(EventsHelper).GetMethod("EventHandlerFactory")!;
}
