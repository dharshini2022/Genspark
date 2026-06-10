using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class SalesTotalsByAmount
{
    public decimal? Saleamount { get; set; }

    public int? Orderid { get; set; }

    public string? Companyname { get; set; }

    public DateTime? Shippeddate { get; set; }
}
