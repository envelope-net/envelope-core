#if NET6_0_OR_GREATER
using Envelope.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Serializer.JsonConverters;

public class EnvironmentInfoJsonConverter : JsonConverter<EnvironmentInfo>
{
	private static readonly Type _dateTimeOffset = typeof(DateTimeOffset);

	public override void Write(Utf8JsonWriter writer, EnvironmentInfo value, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Read only converter");
	}

	public override EnvironmentInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return default;
		}

		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException();
		}
		else
		{
			var stringComparison = options.PropertyNameCaseInsensitive
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;

			string? runningEnvironment = null;
			DateTimeOffset? createdUtc = null;
			string? frameworkDescription = null;
			string? targetFramework = null;
			string? clrVersion = null;
			string? entryAssemblyName = null;
			string? entryAssemblyVersion = null;
			string? baseDirectory = null;
			string? machineName = null;
			string? processName = null;
			int? processId = null;
			string? currentAppDomainName = null;
			bool? is64BitOperatingSystem = null;
			bool? is64BitProcess = null;
			string? operatingSystemPlatform = null;
			string? operatingSystemVersion = null;
			string? operatingSystemArchitecture = null;
			string? processArchitecture = null;
			string? commandLine = null;
			string? applicationName = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					return new EnvironmentInfo(
						runningEnvironment,
						createdUtc,
						frameworkDescription,
						targetFramework,
						clrVersion,
						entryAssemblyName,
						entryAssemblyVersion,
						baseDirectory,
						machineName,
						processName,
						processId,
						currentAppDomainName,
						is64BitOperatingSystem,
						is64BitProcess,
						operatingSystemPlatform,
						operatingSystemVersion,
						operatingSystemArchitecture,
						processArchitecture,
						commandLine,
						applicationName!);
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case var name when string.Equals(name, nameof(EnvironmentInfo.RunningEnvironment), stringComparison):
							runningEnvironment = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.CreatedUtc), stringComparison):
							createdUtc = reader.TokenType == JsonTokenType.Null ? null : ((JsonConverter<DateTimeOffset>)options.GetConverter(_dateTimeOffset)).Read(ref reader, _dateTimeOffset, options);
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.FrameworkDescription), stringComparison):
							frameworkDescription = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.TargetFramework), stringComparison):
							targetFramework = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.CLRVersion), stringComparison):
							clrVersion = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.EntryAssemblyName), stringComparison):
							entryAssemblyName = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.EntryAssemblyVersion), stringComparison):
							entryAssemblyVersion = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.BaseDirectory), stringComparison):
							baseDirectory = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.MachineName), stringComparison):
							machineName = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.ProcessName), stringComparison):
							processName = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.ProcessId), stringComparison):
							processId = (reader.TokenType != JsonTokenType.Null && reader.TryGetInt32(out var pid)) ? pid : null;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.CurrentAppDomainName), stringComparison):
							currentAppDomainName = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.Is64BitOperatingSystem), stringComparison):
							is64BitOperatingSystem = reader.TokenType == JsonTokenType.Null ? null : reader.GetBoolean();
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.Is64BitProcess), stringComparison):
							is64BitProcess = reader.TokenType == JsonTokenType.Null ? null : reader.GetBoolean();
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.OperatingSystemPlatform), stringComparison):
							operatingSystemPlatform = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.OperatingSystemVersion), stringComparison):
							operatingSystemVersion = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.OperatingSystemArchitecture), stringComparison):
							operatingSystemArchitecture = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.ProcessArchitecture), stringComparison):
							processArchitecture = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.CommandLine), stringComparison):
							commandLine = reader.GetString()!;
							break;
						case var name when string.Equals(name, nameof(EnvironmentInfo.ApplicationName), stringComparison):
							applicationName = reader.GetString()!;
							break;
					}
				}
			}

			return default;
		}
	}
}
#endif
