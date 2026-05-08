using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    /// <summary>
    /// Abstract base controller for all API controllers in this application.
    ///
    /// OOP Principles Applied:
    ///   - Inheritance: AdminController, CustomerController, OperatorController all
    ///     inherit from this class instead of repeating shared logic.
    ///   - Polymorphism: Each derived controller can override or extend behavior
    ///     while sharing the base GetCurrentUserId() helper.
    ///   - Encapsulation: The JWT claim parsing logic is encapsulated here once,
    ///     not spread across 3 files.
    ///   - Abstraction: Controllers depend on this contract rather than re-implementing
    ///     the same claim lookup.
    /// </summary>
    public abstract class AppControllerBase : ControllerBase
    {
        /// <summary>
        /// Extracts the authenticated user's numeric ID from the JWT claims.
        /// This was previously duplicated in every controller as GetUserId() / GetAdminId().
        /// </summary>
        protected int GetCurrentUserId()
            => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
