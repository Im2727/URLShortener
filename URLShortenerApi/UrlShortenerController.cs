using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using URLShortenerApi.Models;

namespace URLShortenerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

            var existing = _db.UrlMappings.FirstOrDefault(x => x.OriginalUrl == request.Url);
            if (existing != null)
                return Ok(new { shortUrl = baseUrl + existing.Code });

            string code;
            do
            {
                code = GenerateCode(request.Url);
            } while (_db.UrlMappings.Any(x => x.Code == code));

            var mapping = new UrlMapping { Code = code, OriginalUrl = request.Url };
            _db.UrlMappings.Add(mapping);
            _db.SaveChanges();
            return Ok(new { shortUrl = baseUrl + code });
        }

        [HttpGet("shorten/{*url}")]
        public IActionResult ShortenUrlGet(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("URL is required.");

            var decodedUrl = System.Net.WebUtility.UrlDecode(url);
            var existing = _db.UrlMappings.FirstOrDefault(x => x.OriginalUrl == decodedUrl);
            if (existing != null)
                return Ok(new { shortUrl = baseUrl + existing.Code });

            string code;
            do
            {
                code = GenerateCode(decodedUrl);
            } while (_db.UrlMappings.Any(x => x.Code == code));

            var mapping = new UrlMapping { Code = code, OriginalUrl = decodedUrl };
            _db.UrlMappings.Add(mapping);
            _db.SaveChanges();
            return Ok(new { shortUrl = baseUrl + code });
        }

        [HttpGet("{code}")]
        [Route("/{code}")]
        public IActionResult RedirectToOriginal(string code)
        {
            var mapping = _db.UrlMappings.FirstOrDefault(x => x.Code == code);
            if (mapping != null)
                return Redirect(mapping.OriginalUrl);
            return NotFound();
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
    }
}
