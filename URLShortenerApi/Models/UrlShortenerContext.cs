using Microsoft.EntityFrameworkCore;

namespace URLShortenerApi.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string OriginalUrl { get; set; }
        public int RedirectCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }

    public class UrlShortenerContext : DbContext
    {
        public UrlShortenerContext(DbContextOptions<UrlShortenerContext> options) : base(options) { }
        public DbSet<UrlMapping> UrlMappings { get; set; }
    }
}
