using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class Newuser
{
    public string Username { get; set; } = null!;

    public string? Password { get; set; }

    public string? Role { get; set; }
}
