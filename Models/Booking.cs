using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Event is required")]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Display(Name = "Booking Reference")]
        public string BookingReference { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        // Navigation properties
        public Venue? Venue { get; set; }
        public Event? Event { get; set; }
    }
}
