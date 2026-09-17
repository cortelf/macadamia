using System.Buffers;

namespace Nats.Reply.Core;

public interface INatsMessageDeserializer
{
    object? Deserialize(in ReadOnlySequence<byte> buffer, Type type);
}