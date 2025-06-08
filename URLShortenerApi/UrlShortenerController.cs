using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace URLShortenerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlShortenerController : ControllerBase
    {
        // In-memory store for URL mappings
        private static readonly ConcurrentDictionary<string, string> urlMap = new();
        private static readonly ConcurrentDictionary<string, string> reverseMap = new();
        private static readonly string baseUrl = "http://localhost:5016/"; // Root URL for short links

        [HttpPost("shorten")]
        public IActionResult ShortenUrl([FromBody] UrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("URL is required.");

            // Prevent repetitions
            if (reverseMap.TryGetValue(request.Url, out var existingCode))
            {
                return Ok(new { shortUrl = baseUrl + existingCode });
            }

            string code;
            do
            {
                code = GenerateCode(request.Url);
            } while (urlMap.ContainsKey(code));

            urlMap[code] = request.Url;
            reverseMap[request.Url] = code;
            return Ok(new { shortUrl = baseUrl + code });
        }

        [HttpGet("shorten/{*url}")]
        public IActionResult ShortenUrlGet(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("URL is required.");

            var decodedUrl = System.Net.WebUtility.UrlDecode(url);

            // Prevent repetitions
            if (reverseMap.TryGetValue(decodedUrl, out var existingCode))
            {
                return Ok(new { shortUrl = baseUrl + existingCode });
            }

            string code;
            do
            {
                code = GenerateCode(decodedUrl);
            } while (urlMap.ContainsKey(code));

            urlMap[code] = decodedUrl;
            reverseMap[decodedUrl] = code;
            return Ok(new { shortUrl = baseUrl + code });
        }

        [HttpGet("{code}")]
        [Route("/{code}")]
        public IActionResult RedirectToOriginal(string code)
        {
            if (urlMap.TryGetValue(code, out var originalUrl))
            {
                return Redirect(originalUrl);
            }
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
