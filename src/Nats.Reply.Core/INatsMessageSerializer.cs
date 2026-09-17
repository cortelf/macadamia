using System.Buffers;

namespace Nats.Reply.Core;

public interface INatsMessageSerializer
{
    public void Serialize(IBufferWriter<byte> bufferWriter, object? value);
}