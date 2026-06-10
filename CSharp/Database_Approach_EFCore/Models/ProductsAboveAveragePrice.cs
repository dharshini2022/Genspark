using System;
using System.Collections.Generic;

namespace Database_Approach_EFCore.Models;

public partial class ProductsAboveAveragePrice
{
    public string? Productname { get; set; }

    public decimal? Unitprice { get; set; }
}
