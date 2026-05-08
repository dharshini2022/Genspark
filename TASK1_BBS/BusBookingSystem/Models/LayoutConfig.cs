namespace BusBookingSystem.Models
{
    /// <summary>
    /// Represents the deserialized JSON structure of a bus seat layout.
    /// Declared as internal sealed to enforce Encapsulation — this class is
    /// only needed within the assembly (not exposed as public API) and is
    /// not intended to be inherited or extended.
    /// Moved from CustomerService.cs to the Models layer (correct responsibility).
    /// </summary>
    internal sealed class LayoutConfig
    {
        public string[][]? Seats { get; set; }
    }
}
