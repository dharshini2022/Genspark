using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class Region
{
    public int Regionid { get; set; }

    public string Regiondescription { get; set; } = null!;

    public virtual ICollection<Territory> Territories { get; set; } = new List<Territory>();
}
