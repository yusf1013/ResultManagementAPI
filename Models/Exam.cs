using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Exam
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Semester { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Marks { get; set; }
    }
}