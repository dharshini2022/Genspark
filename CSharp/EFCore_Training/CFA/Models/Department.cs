namespace CFA.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        public string Name { get; set; } = String.Empty;

        // One Department -> Many Students
        public ICollection<Student> Students { get; set; } = [];
    }
}