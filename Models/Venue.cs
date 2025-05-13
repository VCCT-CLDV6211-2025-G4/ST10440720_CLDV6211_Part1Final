using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description can't exceed 1000 characters")]
        public string? Description { get; set; }

        public ICollection<Booking>? Bookings { get; set; }

        public bool IsAvailable(DateTime date)
        {
            return Bookings == null || !Bookings.Any(b => b.BookingDate.Date == date.Date);
        }
    }
}