using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;


namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(100, ErrorMessage = "Event name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Venue selection is required")]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = "/images/event-placeholder.jpg";

        // Not mapped: This is used to capture the uploaded file during form submission
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        public ICollection<Booking>? Bookings { get; set; }

        // ✅ NEW: Foreign Key to EventTypes
        [Required(ErrorMessage = "Event type selection is required")]
        [Display(Name = "Event Type")]
        public int EventTypesId { get; set; }

        [ForeignKey("EventTypesId")]
        public EventTypes? EventTypes { get; set; }
    }
}
