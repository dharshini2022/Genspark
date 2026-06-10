using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class Skill
{
    public string Skill1 { get; set; } = null!;

    public string? Skilldescription { get; set; }

    public virtual ICollection<Employeeskill> Employeeskills { get; set; } = new List<Employeeskill>();
}
