using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class ExamResults
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string StudentId { get; set; }
        [Required]
        public int ExamId { get; set; }
        [Required]
        public int ObtainedMark { get; set; }  
        
    }
}