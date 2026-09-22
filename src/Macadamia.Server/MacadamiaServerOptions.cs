using System.ComponentModel.DataAnnotations;

namespace Macadamia.Server;

public class MacadamiaServerOptions
{
    [MinLength(1)]
    public string? QueueGroup { get; set; } = "macadamia";

    [Range(1, int.MaxValue)]
    public int MaxDegreeOfParallelism { get; set; } = int.MaxValue;
}
