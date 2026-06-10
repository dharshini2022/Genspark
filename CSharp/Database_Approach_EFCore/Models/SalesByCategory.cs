using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class SalesByCategory
{
    public int? Categoryid { get; set; }

    public string? Categoryname { get; set; }

    public string? Productname { get; set; }

    public decimal? Productsales { get; set; }
}
