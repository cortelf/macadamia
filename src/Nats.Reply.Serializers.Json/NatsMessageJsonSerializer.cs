using System.Buffers;
using System.Text.Json;
using Nats.Reply.Core;

namespace Nats.Reply.Serializers.Json;

public class NatsMessageJsonSerializer(JsonSerializerOptions options)
    : INatsMessageSerializer, INatsMessageDeserializer
{
    public void Serialize(IBufferWriter<byte> bufferWriter, object? value)
    {
        var writerOptions = new JsonWriterOptions
        {
            Encoder = options.Encoder,
            Indented = options.WriteIndented,
            MaxDepth = options.MaxDepth,
        };
#if NET9_0_OR_GREATER
        writerOptions.IndentCharacter = options.IndentCharacter;
        writerOptions.IndentSize = options.IndentSize;
        writerOptions.NewLine = options.NewLine;
#endif
        using var writer = new Utf8JsonWriter(bufferWriter, writerOptions);
        JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
    }

    public object? Deserialize(in ReadOnlySequence<byte> buffer, Type type)
    {
        var reader = new Utf8JsonReader(buffer);
        return JsonSerializer.Deserialize(ref reader, type, options);
    }
}
