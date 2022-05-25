namespace Envelope.Policy.Internal;

internal delegate bool ResultPredicate<in TResult>(TResult result);
