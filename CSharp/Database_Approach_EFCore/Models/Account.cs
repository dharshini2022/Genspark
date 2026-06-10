using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class Account
{
    public int Accno { get; set; }

    public string Name { get; set; } = null!;

    public int Balance { get; set; }
}
