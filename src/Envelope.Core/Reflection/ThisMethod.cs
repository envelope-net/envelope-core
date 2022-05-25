using Envelope.Extensions;
using System.Reflection;

namespace Envelope.Reflection;

public static class ThisMethod
{
	public static string? GetMethodFullName(bool ignoreAsync = true, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
	{
		return GetMethodBase(ignoreAsync)?.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName);
	}

	public static string? GetPreviousMethodFullName(bool ignoreAsync = true, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
	{
		return GetPreviousMethodBase(ignoreAsync)?.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName);
	}

	public static MethodBase? GetMethodBase(bool ignoreAsync = true)
	{
		return GetMethodBase(0, ignoreAsync, null);
	}

	public static MethodBase? GetPreviousMethodBase(bool ignoreAsync = true)
	{
		return GetMethodBase(-1, ignoreAsync, null);
	}

	internal static string? GetMethodFullName(List<Type> notAssignableFrom, bool ignoreAsync = true, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
	{
		return GetMethodBase(notAssignableFrom, ignoreAsync)?.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName);
	}

	internal static MethodBase? GetMethodBase(List<Type> notAssignableFrom, bool ignoreAsync = true)
	{
		return GetMethodBase(0, ignoreAsync, notAssignableFrom);
	}

	public static List<string> GetMethodsCallStack(bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
	{
		return
			GetMethodBaseCallStack()
				.Select(m => m.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName))
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToList()!;
	}

	internal static List<MethodBase> GetMethodBaseCallStack()
	{
		var result = new List<MethodBase>();
		var stackTrace = new System.Diagnostics.StackTrace(true);
		for (int i = 0; i < stackTrace.FrameCount; ++i)
		{
			var frame = stackTrace.GetFrame(i);
			var callerMethod = frame?.GetMethod();
			if (callerMethod != null && (callerMethod.DeclaringType == null || !TypeHelper.IsDerivedFrom(callerMethod.DeclaringType, typeof(ThisMethod))))
				result.Add(callerMethod);
		}
		return result;
	}

		private static MethodBase? GetMethodBase(int previousFrameIndex, bool ignoreAsync, List<Type>? notAssignableFrom)
		{
			var stackTrace = new System.Diagnostics.StackTrace(true);
			for (int i = 0; i < stackTrace.FrameCount; ++i)
			{
				var frame = stackTrace.GetFrame(i);
				var callerMethod = frame?.GetMethod();
				if (callerMethod != null
					&& (callerMethod.DeclaringType == null ||
					(!TypeHelper.IsDerivedFrom(callerMethod.DeclaringType, typeof(ThisMethod))
					&& (notAssignableFrom == null
						|| notAssignableFrom.Count == 0
						|| (notAssignableFrom.All(x => !x.IsAssignableFrom(callerMethod.DeclaringType))
							&& notAssignableFrom.All(x => !TypeHelper.IsDeclaredIn(callerMethod.DeclaringType, x)))))))
				{
					if (stackTrace.FrameCount - 1 < i - previousFrameIndex)
						return null;

					if (ignoreAsync && callerMethod.IsAsync())
						continue;

					return GetMethodBase(stackTrace, i, -previousFrameIndex, ignoreAsync);
				}
			}
			return null;
		}

		private static MethodBase? GetMethodBase(System.Diagnostics.StackTrace stackTrace, int fromIndex, int framesToSkip, bool ignoreAsync)
		{
			int skippedFrames = 0;
			for (int i = fromIndex; i < stackTrace.FrameCount; i++)
			{
				var frame = stackTrace.GetFrame(i);
				var method = frame?.GetMethod();

				if (method == null || (ignoreAsync && method.IsAsync()))
					continue;

				if (skippedFrames < framesToSkip)
				{
					skippedFrames++;
					continue;
				}

				return method;
			}

			return null;
		}
}
