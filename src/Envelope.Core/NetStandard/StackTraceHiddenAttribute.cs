#if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

namespace System.Diagnostics
{
	/// <summary>
	/// Types and Methods attributed with StackTraceHidden will be omitted from the stack trace text shown in StackTrace.ToString()
	/// and Exception.StackTrace
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct, Inherited = false)]
	public sealed class StackTraceHiddenAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StackTraceHiddenAttribute"/> class.
		/// </summary>
		public StackTraceHiddenAttribute() { }
	}
}

#endif
