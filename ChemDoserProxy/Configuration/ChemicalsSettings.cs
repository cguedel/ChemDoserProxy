using System.ComponentModel.DataAnnotations;

namespace ChemDoserProxy.Configuration;

public class ChemicalsSettings
{
    [Required]
    public string StateFile { get; set; } = null!;

    public int ChlorPureCapacity { get; set; }

    public int pHMinusCapacity { get; set; }

    public int pHPlusCapacity { get; set; }

    public int FlocPlusCCapacity { get; set; }
}
