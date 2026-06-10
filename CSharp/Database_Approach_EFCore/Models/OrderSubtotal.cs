using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class OrderSubtotal
{
    public int? Orderid { get; set; }

    public decimal? Subtotal { get; set; }
}
