namespace LibrarySystem.Models;

public class Member
{
    public enum membershipType
    {
        Basic = 1,
        Student = 2,
        Premium = 3
    }
    public enum memberRole
    {
        Admin = 1,
        Member = 2
    }
    public int MemberId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Password {get; set;} = string.Empty;
    public string Email { get; set; } = string.Empty;
    public membershipType MembershipType { get; set; }
    public memberRole Role {get; set;}

    public bool IsActive { get; set; } = true;

    public DateTime JoinedAt { get; set; } = DateTime.Now;

    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public ICollection<Fine> FinePayments { get; set; } = new List<Fine>();
    public ICollection<DamageLog>? DamageLogs { get; set; }

    public Member()
    {
        
    }
}