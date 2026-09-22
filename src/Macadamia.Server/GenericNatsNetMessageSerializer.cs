using System.Buffers;
using NATS.Client.Core;
using Macadamia.Core;

namespace Macadamia.Server;

internal sealed class GenericNatsNetMessageSerializer<T>(
    INatsMessageSerializer serializer, INatsMessageDeserializer deserializer)
    : INatsSerialize<T>, INatsDeserialize<T>
    where T : class
{
    public void Serialize(IBufferWriter<byte> bufferWriter, T value)
    {
        serializer.Serialize(bufferWriter, value);
    }

    public T? Deserialize(in ReadOnlySequence<byte> buffer)
    {
        return deserializer.Deserialize(in buffer, typeof(T)) as T;
    }
}