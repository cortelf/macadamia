using System.Buffers;

namespace Macadamia.Core;

public interface INatsMessageSerializer
{
    public void Serialize(IBufferWriter<byte> bufferWriter, object? value);
}