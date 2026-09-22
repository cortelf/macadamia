using System.Buffers;

namespace Macadamia.Core;

public interface INatsMessageDeserializer
{
    object? Deserialize(in ReadOnlySequence<byte> buffer, Type type);
}