namespace LibrarySystem.Models
{
    public class DamageLog
    {
        public enum DamageStatus
        {
            Damage = 1,
            Lost = 2
        }
        public int DamageId {get; set;}
        public int MemberId {get; set;}
        public int CopyId{get; set;}
        public string Description {get; set;} = string.Empty;
        public DateTime DateOfEntry {get; set;} = DateTime.Now;
        public DamageStatus status {get; set;} = DamageStatus.Damage;
        public Member? Member { get; set; }
        public BookCopy? BookCopy { get; set; }

        public DamageLog()
        {
        }
    }
}