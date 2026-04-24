using Microsoft.EntityFrameworkCore;
using BusBookingSystem.Data;
using BusBookingSystem.Models;

public class RouteFixer
{
    public static async Task FixRoutes(AppDbContext db)
    {
        var routes = await db.Routes.ToListAsync();
        int added = 0;
        foreach (var r in routes)
        {
            var reverseExists = routes.Any(rev => 
                rev.SourceCity.ToLower() == r.DestinationCity.ToLower() && 
                rev.DestinationCity.ToLower() == r.SourceCity.ToLower());
            
            if (!reverseExists)
            {
                db.Routes.Add(new BusRoute
                {
                    SourceCity = r.DestinationCity,
                    DestinationCity = r.SourceCity,
                    CreatedByAdminId = r.CreatedByAdminId,
                    IsActive = true
                });
                added++;
            }
        }
        if (added > 0) await db.SaveChangesAsync();
        Console.WriteLine($"Added {added} missing reverse routes.");
    }
}
