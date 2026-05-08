from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import cm
from reportlab.lib import colors
from reportlab.platypus import (SimpleDocTemplate, Paragraph, Spacer, Table,
                                 TableStyle, PageBreak, HRFlowable)
from reportlab.lib.enums import TA_LEFT, TA_CENTER, TA_JUSTIFY

OUTPUT = "BusBookingSystem/Dharshini_K_Oops_Project.pdf"

doc = SimpleDocTemplate(
    OUTPUT, pagesize=A4,
    rightMargin=2*cm, leftMargin=2*cm,
    topMargin=2.2*cm, bottomMargin=2*cm
)

W = A4[0] - 4*cm   # usable width

# ── Colour Palette ────────────────────────────────────────────────────────────
NAVY   = colors.HexColor("#1B2A4A")
TEAL   = colors.HexColor("#1A7F8E")
MINT   = colors.HexColor("#E6F7F9")
GOLD   = colors.HexColor("#F0A500")
LGRAY  = colors.HexColor("#F4F6F9")
DGRAY  = colors.HexColor("#555555")
GREEN  = colors.HexColor("#2E7D32")
RED    = colors.HexColor("#C62828")
WHITE  = colors.white

# ── Styles ────────────────────────────────────────────────────────────────────
styles = getSampleStyleSheet()

def S(name, **kw):
    return ParagraphStyle(name, **kw)

TITLE   = S("TITLE",   fontName="Helvetica-Bold",   fontSize=26, textColor=WHITE,   alignment=TA_CENTER, spaceAfter=4)
SUB1    = S("SUB1",    fontName="Helvetica",         fontSize=13, textColor=WHITE,   alignment=TA_CENTER)
H1      = S("H1",      fontName="Helvetica-Bold",   fontSize=15, textColor=NAVY,    spaceBefore=14, spaceAfter=4)
H2      = S("H2",      fontName="Helvetica-Bold",   fontSize=12, textColor=TEAL,    spaceBefore=10, spaceAfter=3)
BODY    = S("BODY",    fontName="Helvetica",         fontSize=9.5, textColor=DGRAY,  leading=15, alignment=TA_JUSTIFY, spaceAfter=5)
CODE    = S("CODE",    fontName="Courier",           fontSize=8.2, textColor=NAVY,   leading=13, leftIndent=8, spaceAfter=2)
GOOD    = S("GOOD",    fontName="Helvetica-Bold",   fontSize=9.5, textColor=GREEN,  spaceBefore=3, spaceAfter=3)
BAD     = S("BAD",     fontName="Helvetica-Bold",   fontSize=9.5, textColor=RED,    spaceBefore=3, spaceAfter=3)
CAPTION = S("CAPTION", fontName="Helvetica-Oblique", fontSize=8,  textColor=DGRAY,  spaceAfter=8)
BULLET  = S("BULLET",  fontName="Helvetica",         fontSize=9.5, textColor=DGRAY,  leftIndent=14, leading=14, spaceAfter=2)

def hr():  return HRFlowable(width="100%", thickness=0.5, color=TEAL, spaceAfter=6, spaceBefore=6)
def sp(n=6): return Spacer(1, n)

def section_title(txt):
    return [Paragraph(txt, H1), hr()]

def code_block(lines):
    """Render a list of strings as a shaded code block."""
    rows = [[Paragraph(l, CODE)] for l in lines]
    t = Table(rows, colWidths=[W])
    t.setStyle(TableStyle([
        ("BACKGROUND", (0,0),(-1,-1), LGRAY),
        ("BOX",        (0,0),(-1,-1), 0.5, TEAL),
        ("TOPPADDING",   (0,0),(-1,-1), 5),
        ("BOTTOMPADDING",(0,0),(-1,-1), 5),
        ("LEFTPADDING",  (0,0),(-1,-1), 8),
    ]))
    return t

def good(txt): return Paragraph(f"✔ {txt}", GOOD)
def bad(txt):  return Paragraph(f"✘ {txt}", BAD)
def bul(txt):  return Paragraph(f"• {txt}", BULLET)

# ── Cover Page ────────────────────────────────────────────────────────────────
cover_data = [[Paragraph("Bus Booking System", TITLE)],
              [Paragraph("OOP Design Review — C# ASP.NET Core", SUB1)],
              [Spacer(1, 0.3*cm)],
              [Paragraph("Dharshini K", SUB1)],
              [Paragraph("May 2026  |  Presidio × Genspark Training", SUB1)]]

cover = Table(cover_data, colWidths=[W])
cover.setStyle(TableStyle([
    ("BACKGROUND",    (0,0),(-1,-1), NAVY),
    ("TOPPADDING",    (0,0),(-1,-1), 18),
    ("BOTTOMPADDING", (0,0),(-1,-1), 18),
    ("LEFTPADDING",   (0,0),(-1,-1), 20),
    ("RIGHTPADDING",  (0,0),(-1,-1), 20),
    ("ROUNDEDCORNERS",(0,0),(-1,-1), 8),
]))

story = [
    sp(50), cover, sp(30),
    Paragraph("Project: Bus Booking System (BBS) — Full-stack application built with ASP.NET Core 8 "
              "(backend) and Angular 18 (frontend). The backend follows a layered architecture: "
              "Models → Interfaces → Services → Controllers. This document analyses how OOP principles "
              "were applied, what was done well, and what could be improved.", BODY),
    PageBreak()
]

# ══════════════════════════════════════════════════════════════════════════════
# 1. ABSTRACT CLASS
# ══════════════════════════════════════════════════════════════════════════════
story += section_title("1. Abstract Class")
story += [
    Paragraph("An <b>abstract class</b> defines a contract with optional shared implementation. "
              "Concrete subclasses must fill in the blanks while inheriting ready-made members.", BODY),
]

story += [Paragraph("1.1  BaseEntity — The Domain Root", H2)]
story += [
    Paragraph("Every EF Core entity in the project inherits from <b>BaseEntity</b>. "
              "This single class removes the need to redeclare <i>Id</i> and <i>CreatedAt</i> "
              "in all 8+ model files.", BODY),
    code_block([
        "// Models/BaseEntity.cs",
        "public abstract class BaseEntity",
        "{",
        "    public int Id { get; set; }",
        "    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;",
        "}",
        "",
        "// All domain models inherit it:",
        "public class User        : BaseEntity { ... }   // Id + CreatedAt free",
        "public class Bus         : BaseEntity { ... }   // Id + CreatedAt free",
        "public class BusOperator : BaseEntity { ... }   // Id + CreatedAt free",
        "public class Booking     : BaseEntity { ... }   // Id + CreatedAt free",
        "public class BusSchedule : BaseEntity { ... }   // Id + CreatedAt free",
    ]),
    good("DRY — eliminated 16+ redundant property declarations across model files."),
    good("'abstract' keyword prevents accidental direct instantiation of BaseEntity."),
    good("EF Core respects the abstract modifier — no 'BaseEntities' table is created."),
    sp(4),
]

story += [Paragraph("1.2  AppControllerBase — The Shared Controller Root", H2)]
story += [
    Paragraph("Three API controllers (Admin, Customer, Operator) all needed to read the JWT "
              "claim <i>NameIdentifier</i> as an integer user-id. Before refactoring this was "
              "duplicated as <code>GetUserId()</code> in each file. Now it lives once in an "
              "abstract base controller.", BODY),
    code_block([
        "// Controllers/AppControllerBase.cs",
        "public abstract class AppControllerBase : ControllerBase",
        "{",
        "    protected int GetCurrentUserId()",
        '        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);',
        "}",
        "",
        "// Concrete controllers simply inherit:",
        "public class AdminController    : AppControllerBase { ... }",
        "public class CustomerController : AppControllerBase { ... }",
        "public class OperatorController : AppControllerBase { ... }",
    ]),
    good("GetCurrentUserId() declared once — used in all three controllers via inheritance."),
    good("'abstract' + ': ControllerBase' combines framework contract with custom logic cleanly."),
    sp(4),
]

story += [Paragraph("What Could Be Better", H2)]
story += [
    bad("BaseEntity does not define any abstract methods — it is technically more of a "
        "concrete base class. Adding an abstract Validate() method would force each model "
        "to implement domain-level validation."),
    bad("Payment model does NOT inherit BaseEntity (it redeclares public int Id manually). "
        "Inconsistent base class usage across the domain."),
    code_block([
        "// CURRENT (inconsistent):",
        "public class Payment",
        "{",
        "    public int Id { get; set; }   // ← manually redeclared!",
        "    ...",
        "}",
        "",
        "// BETTER:",
        "public class Payment : BaseEntity  { ... }",
    ]),
    sp(6),
]

# ══════════════════════════════════════════════════════════════════════════════
# 2. PARTIAL CLASS
# ══════════════════════════════════════════════════════════════════════════════
story += [PageBreak()]
story += section_title("2. Partial Class")
story += [
    Paragraph("A <b>partial class</b> splits one class definition across multiple files. "
              "It is most useful when a class is very large or when generated code must coexist "
              "with hand-written code.", BODY),
    Paragraph("2.1  Usage in the Project", H2),
    Paragraph("Partial classes were <b>not explicitly used</b> in this project. "
              "The EF Core toolchain generates migration files (in the Migrations/ folder) "
              "that internally use partial classes to separate the designer snapshot "
              "from the migration logic — but this is auto-generated, not hand-crafted.", BODY),
    Paragraph("2.2  Where It Could Have Helped", H2),
    Paragraph("The two largest service files — <b>CustomerService.cs (26 KB)</b> and "
              "<b>OperatorService.cs (27 KB)</b> — contain 400–600 lines each. "
              "Partial classes would split these into logical sub-files without breaking "
              "the single-class guarantee that DI requires.", BODY),
    code_block([
        "// PROPOSED SPLIT for CustomerService:",
        "",
        "// CustomerService.Search.cs",
        "public partial class CustomerService : ICustomerService",
        "{",
        "    public async Task<List<BusSearchResultDto>> SearchBusesAsync(...) { ... }",
        "    public async Task<BusSearchResultDto> GetScheduleDetailsAsync(...) { ... }",
        "}",
        "",
        "// CustomerService.Booking.cs",
        "public partial class CustomerService",
        "{",
        "    public async Task<SeatBlockResponseDto> BlockSeatsAsync(...) { ... }",
        "    public async Task<BookingResponseDto>   CreateBookingAsync(...) { ... }",
        "    public async Task<CancelResponseDto>    CancelBookingAsync(...) { ... }",
        "}",
        "",
        "// CustomerService.Profile.cs",
        "public partial class CustomerService",
        "{",
        "    public async Task<ProfileDto> GetProfileAsync(...) { ... }",
        "    public async Task<ProfileDto> UpdateProfileAsync(...) { ... }",
        "}",
    ]),
    good("DI registration stays the same — services.AddScoped<ICustomerService, CustomerService>()"),
    good("Each partial file has a clear, single responsibility — easier to navigate."),
    bad("Not applied in the current codebase — both large service files are monolithic single files."),
    sp(6),
]

# ══════════════════════════════════════════════════════════════════════════════
# 3. MODEL PREPARATION (EF Core + DTOs)
# ══════════════════════════════════════════════════════════════════════════════
story += [PageBreak()]
story += section_title("3. Model Preparation")

story += [Paragraph("3.1  Domain Models (EF Core Entities)", H2)]
story += [
    Paragraph("The project correctly separates domain entities from API payload shapes. "
              "EF Core navigation properties use lazy-safe initializers and nullable annotations.", BODY),
    code_block([
        "// Models/Booking.cs — rich domain model",
        "public class Booking : BaseEntity",
        "{",
        "    public int    CustomerId  { get; set; }",
        "    public int    ScheduleId  { get; set; }",
        "    public decimal TotalAmount { get; set; }",
        "    public decimal PlatformFee { get; set; }",
        '    [Column("GST")]',
        "    public decimal Gst         { get; set; }",
        "    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;",
        "    public DateTime BookedAt   { get; set; } = DateTime.UtcNow;",
        "    public decimal? RefundAmount { get; set; }",
        "",
        "    // Navigation properties",
        "    public User         Customer  { get; set; } = null!;",
        "    public BusSchedule  Schedule  { get; set; } = null!;",
        "    public ICollection<BookingPassenger> Passengers { get; set; } = new List<>();",
        "    public Payment? Payment { get; set; }",
        "}",
    ]),
    good("null! guards make intent explicit — EF will always populate navigation properties."),
    good("Enum types (BookingStatus, BusStatus, UserRole) used instead of raw strings."),
    good("Computed/domain fields (BookedAt) kept separate from inherited CreatedAt."),
    sp(4),
]

story += [Paragraph("3.2  Enums as Typed State", H2)]
story += [
    Paragraph("All status fields use strongly-typed enums, preventing magic strings.", BODY),
    code_block([
        "public enum BookingStatus  { Confirmed, Cancelled, Refunded }",
        "public enum BusStatus      { Pending, Active, Down, Removed }",
        "public enum ApprovalStatus { Pending, Approved, Disabled }",
        "public enum UserRole       { Customer, Operator, Admin }",
        "public enum PaymentStatus  { Pending, Success, Failed }",
    ]),
    good("Type-safe state transitions — compiler rejects invalid status values."),
    good("EF Core stores these as integers — efficient in the database."),
    sp(4),
]

story += [Paragraph("3.3  DTOs — Data Transfer Objects", H2)]
story += [
    Paragraph("All 437 lines of DTO definitions are consolidated in a single file "
              "<b>DTOs/Dtos.cs</b>, grouped by domain section with clear comment banners. "
              "This is both a strength and a weakness.", BODY),
    code_block([
        "// DTOs/Dtos.cs — one file, grouped by concern",
        "// ── AUTH ──────────────────────────────────────────────────────────",
        "public class RegisterDto      { ... }",
        "public class LoginDto         { ... }",
        "public class AuthResponseDto  { ... }",
        "",
        "// ── BOOKING ───────────────────────────────────────────────────────",
        "public class CreateBookingDto { ... }",
        "public class BookingResponseDto { ... }",
        "public class BookingListDto   { ... }",
        "",
        "// ── EMAIL DTOs (moved from EmailService.cs — OOP Encapsulation fix)",
        "public class BookingEmailDto      { ... }",
        "public class CancellationEmailDto { ... }",
    ]),
    good("Email DTOs were correctly migrated from EmailService.cs to the DTOs layer."),
    good("Grouping by concern with comment banners makes navigation easy."),
    bad("All 30+ DTOs in one file — any change requires opening a 437-line file."),
    bad("No base DTO class — RegisterDto and OperatorRegisterDto share Name/Email/Phone/Password "
        "but there is no shared parent class to enforce DRY."),
    sp(4),
]

story += [Paragraph("3.4  Computed Properties in DTOs", H2)]
story += [
    code_block([
        "// BookingListDto — computed property (no setter, derived from stored values)",
        "public class BookingListDto",
        "{",
        "    public decimal TotalAmount { get; set; }",
        "    public decimal PlatformFee { get; set; }",
        "    public decimal Gst         { get; set; }",
        "    public decimal GrandTotal  => TotalAmount + PlatformFee + Gst;  // computed!",
        "}",
    ]),
    good("GrandTotal is computed on the fly — no risk of stale cached value in the DTO."),
    sp(6),
]

# ══════════════════════════════════════════════════════════════════════════════
# 4. INTERFACES & ABSTRACTION
# ══════════════════════════════════════════════════════════════════════════════
story += [PageBreak()]
story += section_title("4. Interfaces & Abstraction")
story += [
    Paragraph("Every service in the project has a corresponding interface. Controllers depend only "
              "on the interface — never on the concrete class directly.", BODY),
    code_block([
        "// IAdminService.cs — pure contract, no implementation",
        "public interface IAdminService",
        "{",
        "    Task<List<OperatorListDto>> GetOperatorsAsync(string? status);",
        "    Task<MessageResponseDto>   ApproveOperatorAsync(int adminId, int operatorId);",
        "    Task<MessageResponseDto>   DisableOperatorAsync(int adminId, int operatorId);",
        "    Task<List<BusPendingDto>>  GetBusesAsync(string? status);",
        "    Task<RouteDto>             CreateRouteAsync(int adminId, CreateRouteDto dto);",
        "    Task<RevenueDto>           GetRevenueAsync();",
        "    // ... 8 more methods",
        "}",
        "",
        "// AdminController uses the interface — not the class:",
        "public class AdminController : AppControllerBase",
        "{",
        "    private readonly IAdminService _svc;",
        "    public AdminController(IAdminService svc) => _svc = svc;   // DI",
        "}",
    ]),
    good("Dependency Injection (Program.cs) wires AdminService → IAdminService at runtime."),
    good("AdminController can be unit-tested by passing a mock IAdminService."),
    good("4 interfaces defined — IAdminService, ICustomerService, IOperatorService, IAuthService."),
    bad("IEmailService exists but EmailService is injected concretely in some places too."),
    sp(6),
]

# ══════════════════════════════════════════════════════════════════════════════
# 5. INHERITANCE & ENCAPSULATION
# ══════════════════════════════════════════════════════════════════════════════
story += section_title("5. Inheritance & Encapsulation")
story += [
    Paragraph("5.1  Inheritance Hierarchy", H2),
    Paragraph("The project applies a clean two-level inheritance tree:", BODY),
]

inh_data = [
    ["Layer", "Base", "Derived Classes"],
    ["Models",      "BaseEntity (abstract)",     "User, Bus, Booking, BusOperator,\nBusSchedule, BusLayout, BusRoute, SeatBlock"],
    ["Controllers", "AppControllerBase (abstract)", "AdminController,\nCustomerController, OperatorController"],
    ["Framework",   "ControllerBase (ASP.NET)",  "AppControllerBase → all controllers"],
]
inh_table = Table(inh_data, colWidths=[3.5*cm, 5*cm, 7.5*cm])
inh_table.setStyle(TableStyle([
    ("BACKGROUND",    (0,0),(-1,0),  NAVY),
    ("TEXTCOLOR",     (0,0),(-1,0),  WHITE),
    ("FONTNAME",      (0,0),(-1,0),  "Helvetica-Bold"),
    ("FONTSIZE",      (0,0),(-1,-1), 8.5),
    ("ROWBACKGROUNDS",(0,1),(-1,-1), [LGRAY, WHITE]),
    ("GRID",          (0,0),(-1,-1), 0.4, colors.HexColor("#CBD5E0")),
    ("TOPPADDING",    (0,0),(-1,-1), 5),
    ("BOTTOMPADDING", (0,0),(-1,-1), 5),
    ("LEFTPADDING",   (0,0),(-1,-1), 7),
    ("VALIGN",        (0,0),(-1,-1), "TOP"),
]))
story += [inh_table, sp(8)]

story += [Paragraph("5.2  Encapsulation", H2)]
story += [
    Paragraph("All model properties use C# auto-property syntax with sensible defaults. "
              "The service layer encapsulates all business logic — controllers never touch "
              "DbContext or raw SQL.", BODY),
    code_block([
        "// Encapsulation in action — controller knows nothing about DB:",
        "[HttpPut('operators/{id:int}/approve')]",
        "public async Task<IActionResult> ApproveOperator(int id)",
        "{",
        "    var result = await _svc.ApproveOperatorAsync(GetCurrentUserId(), id);",
        "    return Ok(result);",
        "}",
        "// All EF Core queries live inside AdminService — hidden from controller.",
    ]),
    good("Service layer fully encapsulates data access — controllers are thin."),
    good("JWT claim parsing is encapsulated in AppControllerBase.GetCurrentUserId()."),
    bad("Some service methods are very long (50–80 lines) — internal logic could be "
        "further encapsulated into private helper methods."),
    sp(6),
]

# ══════════════════════════════════════════════════════════════════════════════
# 6. POLYMORPHISM
# ══════════════════════════════════════════════════════════════════════════════
story += [PageBreak()]
story += section_title("6. Polymorphism")
story += [
    Paragraph("Polymorphism lets callers treat different types uniformly. In this project it "
              "appears at two levels:", BODY),
    Paragraph("6.1  Interface Polymorphism (Runtime)", H2),
    code_block([
        "// Program.cs — DI registration",
        "builder.Services.AddScoped<IAdminService,    AdminService>();",
        "builder.Services.AddScoped<ICustomerService, CustomerService>();",
        "builder.Services.AddScoped<IOperatorService, OperatorService>();",
        "",
        "// The ASP.NET runtime resolves the correct concrete class at runtime.",
        "// In a test, you can pass a MockAdminService — same controller code, different behaviour.",
    ]),
    good("Full runtime polymorphism via DI — swap implementation without touching the controller."),
    Paragraph("6.2  Compile-Time (Method Overloading / Expression Bodies)", H2),
    code_block([
        "// Expression-bodied members — concise but polymorphic in spirit:",
        "protected int GetCurrentUserId()",
        '    => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);',
        "",
        "// Constructor injection shorthand across all controllers:",
        "public AdminController(IAdminService svc)    => _svc = svc;",
        "public CustomerController(ICustomerService svc) => _svc = svc;",
        "public OperatorController(IOperatorService svc) => _svc = svc;",
    ]),
    bad("No method overriding (virtual/override) used in the domain — all services implement "
        "their interface from scratch. A shared abstract BaseService<T> with common CRUD "
        "could reduce boilerplate."),
    sp(6),
]

# ══════════════════════════════════════════════════════════════════════════════
# 7. SUMMARY TABLE
# ══════════════════════════════════════════════════════════════════════════════
story += section_title("7. Overall Assessment Summary")

rows = [
    ["OOP Concept",         "Applied?",   "Rating",   "Key Note"],
    ["Abstract Class\n(BaseEntity)",    "✔ Yes",  "★★★★★", "All 8 models inherit — perfect usage"],
    ["Abstract Class\n(AppControllerBase)", "✔ Yes","★★★★☆","Removes JWT duplication across 3 controllers"],
    ["Partial Class",       "✘ No",      "★★☆☆☆",  "Large service files need splitting"],
    ["Inheritance\n(Models)", "✔ Yes",   "★★★★☆",  "Payment class missed — does not extend BaseEntity"],
    ["Inheritance\n(Controllers)", "✔ Yes","★★★★★","Clean 3-level chain: ControllerBase→AppControllerBase→Concrete"],
    ["Interface / Abstraction","✔ Yes",  "★★★★☆",  "4 interfaces, DI wired; minor gaps in EmailService"],
    ["Encapsulation",       "✔ Yes",     "★★★★☆",  "Thin controllers; service methods slightly long"],
    ["Polymorphism",        "Partial",   "★★★☆☆",  "Runtime via DI; no virtual/override in domain"],
    ["Model Prep (Entities)","✔ Yes",   "★★★★★",  "Rich domain models with enums, nullability, nav props"],
    ["Model Prep (DTOs)",   "Partial",   "★★★☆☆",  "One big Dtos.cs file; no base DTO hierarchy"],
]
col_w = [4*cm, 2.2*cm, 2.5*cm, 7.3*cm]
tbl = Table(rows, colWidths=col_w)
tbl.setStyle(TableStyle([
    ("BACKGROUND",    (0,0),(-1,0),  NAVY),
    ("TEXTCOLOR",     (0,0),(-1,0),  WHITE),
    ("FONTNAME",      (0,0),(-1,0),  "Helvetica-Bold"),
    ("FONTSIZE",      (0,0),(-1,-1), 8),
    ("ROWBACKGROUNDS",(0,1),(-1,-1), [LGRAY, WHITE]),
    ("GRID",          (0,0),(-1,-1), 0.4, colors.HexColor("#CBD5E0")),
    ("TOPPADDING",    (0,0),(-1,-1), 5),
    ("BOTTOMPADDING", (0,0),(-1,-1), 5),
    ("LEFTPADDING",   (0,0),(-1,-1), 7),
    ("VALIGN",        (0,0),(-1,-1), "TOP"),
]))
story += [tbl, sp(8)]

# ══════════════════════════════════════════════════════════════════════════════
# 8. RECOMMENDATIONS
# ══════════════════════════════════════════════════════════════════════════════
story += section_title("8. Recommendations for Next Iteration")
recs = [
    "Fix Payment model to inherit BaseEntity — removes the one inconsistency in the domain hierarchy.",
    "Split CustomerService.cs and OperatorService.cs using partial classes — each partial file "
    "covers one domain concern (Search, Booking, Profile, Revenue).",
    "Introduce a base RegisterDto with Name, Email, Phone, Password — RegisterDto and "
    "OperatorRegisterDto both extend it (inheritance in DTOs).",
    "Add a generic BaseService<TEntity> abstract class with shared helpers like "
    "GetOrThrowAsync(int id) — avoids repeating null-check patterns in every service.",
    "Split Dtos.cs into feature folders: DTOs/Auth/, DTOs/Booking/, DTOs/Operator/ etc.",
    "Add abstract Validate() to BaseEntity and implement business rules per model "
    "(e.g., Booking.Validate() checks TotalAmount > 0).",
    "Use virtual/override for polymorphic cancel logic — a base CancelAsync on a "
    "shared service removes the duplicated refund-calculation logic in Admin and Customer services.",
]
for i, r in enumerate(recs, 1):
    story.append(Paragraph(f"{i}.  {r}", BULLET))
    story.append(sp(3))

story += [sp(20),
    Paragraph("— Dharshini K  |  Bus Booking System OOP Review  |  May 2026 —",
              S("footer", fontName="Helvetica-Oblique", fontSize=8,
                textColor=DGRAY, alignment=TA_CENTER))]

doc.build(story)
print(f"PDF saved → {OUTPUT}")
