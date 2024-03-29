﻿namespace Envelope.Infrastructure;

public static class EnvironmentInfoProvider
{
	public static EnvironmentInfo GetEnvironmentInfo(string applicationName)
	{
		return new EnvironmentInfo(
			EnvironmentInfoProviderCache.Instance.RunningEnvironment,
			DateTimeOffset.UtcNow,
			EnvironmentInfoProviderCache.Instance.FrameworkDescription,
			EnvironmentInfoProviderCache.Instance.TargetFramework,
			EnvironmentInfoProviderCache.Instance.CLRVersion,
			EnvironmentInfoProviderCache.Instance.EntryAssemblyName,
			EnvironmentInfoProviderCache.Instance.EntryAssemblyVersion,
			EnvironmentInfoProviderCache.Instance.BaseDirectory,
			EnvironmentInfoProviderCache.Instance.MachineName,
			EnvironmentInfoProviderCache.Instance.ProcessName,
			EnvironmentInfoProviderCache.Instance.ProcessId,
			EnvironmentInfoProviderCache.Instance.CurrentAppDomainName,
			EnvironmentInfoProviderCache.Instance.Is64BitOperatingSystem,
			EnvironmentInfoProviderCache.Instance.Is64BitProcess,
			EnvironmentInfoProviderCache.Instance.OperatingSystemPlatform,
			EnvironmentInfoProviderCache.Instance.OperatingSystemVersion,
			EnvironmentInfoProviderCache.Instance.OperatingSystemArchitecture,
			EnvironmentInfoProviderCache.Instance.ProcessArchitecture,
			EnvironmentInfoProviderCache.Instance.CommandLine,
			applicationName);
	}
}
