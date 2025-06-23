using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventTypes
    {
        [Key]  //This is the Primary Key EF Core expects
        public int EventTypesId { get; set; }

        [Required(ErrorMessage = "Event type name is required")]
        [StringLength(100, ErrorMessage = "Event type name cannot exceed 100 characters")]
        public string Name { get; set; }
    }
}
