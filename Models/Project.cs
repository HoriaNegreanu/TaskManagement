using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class Project
    {
        public int ID { get; set; }

        [StringLength(50)]
        [Required]
        public string? Title { get; set; }
    }
}
