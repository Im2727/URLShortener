using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using URLShortenerApi.Models;

namespace URLShortenerApi.Controllers
{
    [Route("shorten")]
    public class UrlShortenerController : ControllerBase
    {
        private readonly UrlShortenerContext _db;
        private static readonly string baseUrl = "http://localhost:5016/"; // Root URL for short links

        public UrlShortenerController(UrlShortenerContext db)
        {
            _db = db;
        }

        [HttpPost("shorten")]
        public IActionResult ShortenUrl([FromBody] UrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("URL is required.");

            DateTime expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(30);

            // If a custom code is provided, check for conflicts
            if (!string.IsNullOrWhiteSpace(request.CustomCode))
            {
                if (_db.UrlMappings.Any(x => x.Code == request.CustomCode))
                    return Conflict("Custom short code is already in use.");
                var mapping = new UrlMapping { Code = request.CustomCode, OriginalUrl = request.Url, ExpiresAt = expiresAt };
                _db.UrlMappings.Add(mapping);
                _db.SaveChanges();
                return Ok(new { shortUrl = baseUrl + request.CustomCode, expiresAt });
            }

            var existing = _db.UrlMappings.FirstOrDefault(x => x.OriginalUrl == request.Url);
            if (existing != null)
                return Ok(new { shortUrl = baseUrl + existing.Code, expiresAt = existing.ExpiresAt });

            string code;
            do
            {
                code = GenerateCode(request.Url);
            } while (_db.UrlMappings.Any(x => x.Code == code));

            var autoMapping = new UrlMapping { Code = code, OriginalUrl = request.Url, ExpiresAt = expiresAt };
            _db.UrlMappings.Add(autoMapping);
            _db.SaveChanges();
            return Ok(new { shortUrl = baseUrl + code, expiresAt });
        }

        // GET /api/urlshortener/shorten?url={url} for random code
        [HttpGet("shorten")]
        public IActionResult ShortenUrlGet([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("URL is required.");

            var existing = _db.UrlMappings.FirstOrDefault(x => x.OriginalUrl == url);
            if (existing != null)
                return Ok(new { shortUrl = baseUrl + existing.Code });

            string code;
            do
            {
                code = GenerateCode(url);
            } while (_db.UrlMappings.Any(x => x.Code == code));

            var mapping = new UrlMapping { Code = code, OriginalUrl = url };
            _db.UrlMappings.Add(mapping);
            _db.SaveChanges();
            return Ok(new { shortUrl = baseUrl + code });
        }

        // GET /api/urlshortener/shorten/{code}?url={url} for custom code
        [HttpGet("shorten/{code}")]
        public IActionResult ShortenUrlGetCustom([FromRoute] string code, [FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(code))
                return BadRequest("Both code and url are required.");

            if (_db.UrlMappings.Any(x => x.Code == code))
                return Conflict("Custom short code is already in use.");

            var existing = _db.UrlMappings.FirstOrDefault(x => x.OriginalUrl == url);
            if (existing != null)
                return Ok(new { shortUrl = baseUrl + existing.Code });

            var mapping = new UrlMapping { Code = code, OriginalUrl = url };
            _db.UrlMappings.Add(mapping);
            _db.SaveChanges();
            return Ok(new { shortUrl = baseUrl + code });
        }

        // GET /shorten/custom/{customCode}/{*url} for custom code
        [HttpGet("custom/{customCode}/{*url}")]
        public IActionResult ShortenUrlCustomRoute(string customCode, string url, [FromQuery] DateTime? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(customCode))
                return BadRequest("Both custom code and url are required.");

            url = System.Net.WebUtility.UrlDecode(url);
            DateTime exp = expiresAt ?? DateTime.UtcNow.AddDays(30);

            // Check if the custom code is already in use
            if (_db.UrlMappings.Any(x => x.Code == customCode))
                return Conflict("Custom short code is already in use.");

            var mapping = new UrlMapping { Code = customCode, OriginalUrl = url, ExpiresAt = exp };
            _db.UrlMappings.Add(mapping);
            _db.SaveChanges();
            return Ok(new { shortUrl = baseUrl + customCode, expiresAt = exp });
        }

        // GET /shorten/{*url} for random code
        [HttpGet("{*url}")]
        public IActionResult ShortenUrlRoute(string url, [FromQuery] DateTime? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("URL is required.");

            url = System.Net.WebUtility.UrlDecode(url);
            DateTime exp = expiresAt ?? DateTime.UtcNow.AddDays(30);

            var existing = _db.UrlMappings.FirstOrDefault(x => x.OriginalUrl == url);
            if (existing != null)
                return Ok(new { shortUrl = baseUrl + existing.Code, expiresAt = existing.ExpiresAt });

            string code;
            do
            {
                code = GenerateCode(url);
            } while (_db.UrlMappings.Any(x => x.Code == code));

            var mapping = new UrlMapping { Code = code, OriginalUrl = url, ExpiresAt = exp };
            _db.UrlMappings.Add(mapping);
            _db.SaveChanges();
            return Ok(new { shortUrl = baseUrl + code, expiresAt = exp });
        }

        // GET /shorten/analytics/{code} - get redirect count for a short code
        [HttpGet("analytics/{code}")]
        public IActionResult GetAnalytics(string code)
        {
            var mapping = _db.UrlMappings.FirstOrDefault(x => x.Code == code);
            if (mapping == null)
                return NotFound("Short code not found.");
            return Ok(new { code = mapping.Code, redirectCount = mapping.RedirectCount });
        }

        // GET /shorten/original/{code} - get the original URL for a short code
        [HttpGet("original/{code}")]
        public IActionResult GetOriginalUrl(string code)
        {
            var mapping = _db.UrlMappings.FirstOrDefault(x => x.Code == code);
            if (mapping == null)
                return NotFound("Short code not found.");
            return Ok(new { code = mapping.Code, originalUrl = mapping.OriginalUrl, expiresAt = mapping.ExpiresAt });
        }

        private static string GenerateCode(string url)
        {
            // Use SHA256 hash and take first 6 chars for uniqueness
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(url + System.Guid.NewGuid()));
            return Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Substring(0, 6);
        }
    }

    public class UrlRequest
    {
        public string Url { get; set; }
        public string? CustomCode { get; set; } // Optional custom short code
        public DateTime? ExpiresAt { get; set; } // Optional expiration date
    }
}
