namespace CFA.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public string Name { get; set; } = String.Empty;

        public DateTime DateOfBirth {get; set;} = DateTime.Now;

        // Foreign Key
        public int DepartmentId { get; set; }

        // Navigation Property
        public Department Department { get; set; } = null!;  

        // One Student -> One Profile
        public StudentProfile StudentProfile { get; set; } = null!;       
    }
}