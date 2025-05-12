using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Venue name is required")]
        [StringLength(100, ErrorMessage = "Venue name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        // Replace static path with a field that can be updated with Azure Blob URL
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description can't exceed 1000 characters")]
        public string? Description { get; set; }

        public ICollection<Booking>? Bookings { get; set; }

        /// <summary>
        /// Checks if the venue is available on a specific date.
        /// </summary>
        /// <param name="date">The date to check availability for.</param>
        /// <returns>True if available, false if already booked.</returns>
        public bool IsAvailable(DateTime date)
        {
            return Bookings == null || !Bookings.Any(b => b.BookingDate.Date == date.Date);
        }
    }
}
