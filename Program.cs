using EventEase.Data;
using EventEase.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession(); // Enable session

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed the database
await SeedDatabaseAsync(app);

app.Run();


// ---------------------- Seeding Logic ----------------------

async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Apply any pending migrations
        await context.Database.MigrateAsync();

        // Seed only if DB is empty
        if (!await context.Venues.AnyAsync())
        {
            await SeedVenuesAsync(context);
            await SeedEventsAsync(context);
            await SeedBookingsAsync(context);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

async Task SeedVenuesAsync(ApplicationDbContext context)
{
    var venues = new[]
    {
        new Venue
        {
            Name = "Grand Ballroom",
            Location = "123 Main Street, Downtown",
            Capacity = 500,
            ImageUrl = "/images/venue1.jpg",
            Description = "Elegant ballroom with crystal chandeliers"
        },
        new Venue
        {
            Name = "Tech Conference Center",
            Location = "456 Innovation Drive",
            Capacity = 300,
            ImageUrl = "/images/venue2.jpg",
            Description = "Modern conference space with AV equipment"
        },
        new Venue
        {
            Name = "Garden Pavilion",
            Location = "789 Park Avenue",
            Capacity = 150,
            ImageUrl = "/images/venue3.jpg",
            Description = "Outdoor venue with beautiful gardens"
        }
    };

    await context.Venues.AddRangeAsync(venues);
    await context.SaveChangesAsync();
}

async Task SeedEventsAsync(ApplicationDbContext context)
{
    var events = new[]
    {
        new Event
        {
            Name = "Annual Tech Summit",
            Description = "The biggest technology conference of the year",
            ImageUrl = "/images/tech-event.jpg"
        },
        new Event
        {
            Name = "Wedding Expo",
            Description = "Showcase of wedding vendors and services",
            ImageUrl = "/images/wedding-event.jpg"
        },
        new Event
        {
            Name = "Music Festival",
            Description = "Three days of live music performances",
            ImageUrl = "/images/music-event.jpg"
        }
    };

    await context.Events.AddRangeAsync(events);
    await context.SaveChangesAsync();
}

async Task SeedBookingsAsync(ApplicationDbContext context)
{
    var venues = await context.Venues.ToListAsync();
    var events = await context.Events.ToListAsync();

    var bookings = new[]
    {
        new Booking
        {
            VenueId = venues[0].VenueId,
            EventId = events[0].EventId,
            StartTime = DateTime.Now.AddDays(7).Date.AddHours(9),
            EndTime = DateTime.Now.AddDays(7).Date.AddHours(17)
        },
        new Booking
        {
            VenueId = venues[1].VenueId,
            EventId = events[1].EventId,
            StartTime = DateTime.Now.AddDays(14).Date.AddHours(10),
            EndTime = DateTime.Now.AddDays(14).Date.AddHours(16)
        },
        new Booking
        {
            VenueId = venues[2].VenueId,
            EventId = events[2].EventId,
            StartTime = DateTime.Now.AddDays(21).Date.AddHours(12),
            EndTime = DateTime.Now.AddDays(23).Date.AddHours(22)
        }
    };

    await context.Bookings.AddRangeAsync(bookings);
    await context.SaveChangesAsync();
}
