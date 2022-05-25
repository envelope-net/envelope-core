namespace Envelope.Observables;

/// <summary>
/// A connect handle is returned by a non-asynchronous resource that supports
/// disconnection (such as removing an observer, etc.)
/// </summary>
public interface IConnectHandle : IDisposable
{
	/// <summary>
	/// Explicitly disconnect the handle without waiting for it to be disposed. If the 
	/// connection is disconnected, the disconnect will be ignored when the handle is disposed.
	/// </summary>
	void Disconnect();
}
