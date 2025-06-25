using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortenerApi.Controllers;
using URLShortenerApi.Models;
using Xunit;

namespace URLShortenerApi.Tests
{
    public class UrlShortenerControllerTests
    {
        private UrlShortenerController GetControllerWithInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<UrlShortenerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var db = new UrlShortenerContext(options);
            return new UrlShortenerController(db);
        }

        [Fact]
        public void ShortenUrl_PostRandomUrl_ReturnsShortUrl()
        {
            // Arrange
            var controller = GetControllerWithInMemoryDb();
            var request = new UrlRequest { Url = "https://example.com" };

            // Act
            var result = controller.ShortenUrl(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var shortUrlProp = result.Value.GetType().GetProperty("shortUrl");
            Assert.NotNull(shortUrlProp);
            var shortUrl = shortUrlProp.GetValue(result.Value) as string;
            Assert.NotNull(shortUrl);
            Assert.Contains("http://localhost:27/", shortUrl);
        }

        [Fact]
        public void ShortenUrl_PostCustomCode_ReturnsCustomShortUrl()
        {
            // Arrange
            var controller = GetControllerWithInMemoryDb();
            var request = new UrlRequest { Url = "https://example.com", CustomCode = "custom1" };

            // Act
            var result = controller.ShortenUrl(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var shortUrlProp = result.Value.GetType().GetProperty("shortUrl");
            Assert.NotNull(shortUrlProp);
            var shortUrl = shortUrlProp.GetValue(result.Value) as string;
            Assert.NotNull(shortUrl);
            Assert.Equal("http://localhost:27/custom1", shortUrl);
        }
    }
}
