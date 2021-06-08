using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
    public class ItemData
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool Done { get; set; }
    }
}