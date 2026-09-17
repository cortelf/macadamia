using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nats.Reply.Serializers.Json;

public class DefaultNatsMessageJsonSerializer() : NatsMessageJsonSerializer(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters =
    {
        new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.CamelCase, allowIntegerValues: true)
    },
    WriteIndented = true 
})
{
    
}
