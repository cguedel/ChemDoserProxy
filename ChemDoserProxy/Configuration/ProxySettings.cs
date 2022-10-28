using System.ComponentModel.DataAnnotations;

namespace ChemDoserProxy.Configuration;

public class ProxySettings
{
    [Required]
    public string Listen { get; set; } = null!;

    [Required]
    [Range(1, 65535)]
    public int Port { get; set; }

    public string? ForwardHost { get; set; }

    [Range(1, 65535)]
    public int? ForwardPort { get; set; }
}
