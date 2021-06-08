using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class StudentDetails
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string DOB { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public int Semester { get; set; }

        // public bool AccountSuspended { get; set; }
    }
}