namespace Envelope;

public interface IGlobalContext
{
	/// <summary>
	/// Gets a System.DateTime object that is set to the current date and time on this computer, expressed as the local time.
	/// </summary>
	DateTime Now { get; }

	/// <summary>
	/// Gets the current date.
	/// </summary>
	DateTime Today { get; }

	/// <summary>
	/// Gets a System.DateTime object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC)
	/// </summary>
	DateTime UtcNow { get; }

	/// <summary>
	/// Initializes a new instance of the System.Guid structure.
	/// </summary>
	/// <returns>A new GUID object.</returns>
	Guid NewGuid();
}
