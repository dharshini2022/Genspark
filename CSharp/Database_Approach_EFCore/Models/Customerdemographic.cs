using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class Customerdemographic
{
    public string Customertypeid { get; set; } = null!;

    public string? Customerdesc { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
