﻿namespace Envelope.Localization;

public interface IApplicationResources
{
	string GlobalExceptionMessage { get; }
	string DataNotFoundException { get; }
	string DataForbiddenException { get; }
	string OptimisticConcurrencyException { get; }
}
