namespace Envelope.Infrastructure;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IEnvironmentInfo : Serializer.IDictionaryObject
{
	Guid RuntimeUniqueKey { get; }
	
	DateTimeOffset? CreatedUtc { get; set; }

	string? RunningEnvironment { get; }

	string? EntryAssemblyName { get; }

	string? EntryAssemblyVersion { get; }

	string? BaseDirectory { get; }

	string? FrameworkDescription { get; }

	string? TargetFramework { get; }

	string? CLRVersion { get; }

	string? MachineName { get; }
	
	string? ProcessName { get; set; }
	
	int? ProcessId { get; set; }

	string? CurrentAppDomainName { get; }

	bool? Is64BitOperatingSystem { get; }

	bool? Is64BitProcess { get; }

	string? OperatingSystemArchitecture { get; }

	string? OperatingSystemPlatform { get; }

	string? OperatingSystemVersion { get; }

	string? ProcessArchitecture { get; }

	string? CommandLine { get; }
}
