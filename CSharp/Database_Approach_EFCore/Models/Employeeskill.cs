using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class Employeeskill
{
    public int Employeeid { get; set; }

    public string Skill { get; set; } = null!;

    public int Skilllevel { get; set; }

    public virtual Skill SkillNavigation { get; set; } = null!;
}
