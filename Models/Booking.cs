using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using EventEase.Data;

namespace EventEase.Models
{
    [NoDoubleBooking]
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [ForeignKey(nameof(Venue))]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Event is required")]
        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }

        [Display(Name = "Booking Reference")]
        public string BookingReference { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Venue? Venue { get; set; }
        public virtual Event? Event { get; set; }
    }

    // ------------------------------
    // Custom validation attribute
    // ------------------------------
    public class NoDoubleBookingAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var booking = (Booking)validationContext.ObjectInstance;

            var dbContext = (ApplicationDbContext?)validationContext.GetService(typeof(ApplicationDbContext));

            if (dbContext == null)
                return new ValidationResult("Database context not available for validation.");

            // Look for overlapping bookings at the same venue (excluding self for edits)
            var overlapping = dbContext.Bookings
                .Where(b => b.VenueId == booking.VenueId && b.BookingId != booking.BookingId)
                .Where(b => (booking.StartTime < b.EndTime) && (booking.EndTime > b.StartTime))
                .FirstOrDefault();

            if (overlapping != null)
            {
                return new ValidationResult("This venue is already booked during the selected time slot.");
            }

            return ValidationResult.Success;
        }
    }
}
