namespace Envelope.Data;

public interface IBatchWriter : IDisposable
{
}

public interface IBatchWriter<T> : IBatchWriter
{
	void Write(T obj);
}
