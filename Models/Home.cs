using EventEase.Models;
using System.Collections.Generic;

namespace EventEase.Controllers
{
    public class HomeViewModel
    {
        // Basic statistics
        public int VenueCount { get; set; }
        public int EventCount { get; set; }
        public int BookingCount { get; set; }
        public int UpcomingBookingCount { get; set; }
        public int PastBookingCount { get; set; }

        // Recent activity
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Event> UpcomingEvents { get; set; } = new List<Event>();
        public List<Venue> PopularVenues { get; set; } = new List<Venue>();

        // System status
        public bool DatabaseConnected { get; set; }
        public string SystemVersion { get; set; } = "1.0.0";
        public string LastUpdate { get; set; }

        // Helper properties for views
        public bool HasRecentBookings => RecentBookings?.Count > 0;
        public bool HasUpcomingEvents => UpcomingEvents?.Count > 0;
        public bool HasPopularVenues => PopularVenues?.Count > 0;
    }

    public class BookingSummary
    {
        public int BookingId { get; set; }
        public string EventName { get; set; }
        public string VenueName { get; set; }
        public string DateRange { get; set; }
        public string BookingReference { get; set; }
    }

    public class EventSummary
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int BookingCount { get; set; }
    }

    public class VenueSummary
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int BookingCount { get; set; }
        public string ImageUrl { get; set; }
    }

    public class SystemStatus
    {
        public bool DatabaseConnected { get; set; }
        public string Version { get; set; }
        public DateTime LastUpdate { get; set; }
        public int ActiveUsers { get; set; }
    }
}
