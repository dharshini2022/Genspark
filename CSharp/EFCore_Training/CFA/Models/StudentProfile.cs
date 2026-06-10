using System.ComponentModel.DataAnnotations;
namespace CFA.Models
{
    public class StudentProfile
    {
        [Key]
        public int StudentProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = String.Empty;

        [Required]
        [MaxLength(10)]
        public string PhoneNumber { get; set; } = String.Empty;

        // Foreign Key
        [Required]
        public int StudentId { get; set; }
        // Navigation Property
        public Student Student { get; set; } = null!;       
    }
}