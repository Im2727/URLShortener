using Microsoft.AspNetCore.Mvc;
using URLShortenerApi.Models;

namespace URLShortenerApi.Controllers
{
    [Route("{code}")]
    public class RedirectController : ControllerBase
    {
        private readonly UrlShortenerContext _db;
        public RedirectController(UrlShortenerContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult RedirectToOriginal(string code)
        {
            var mapping = _db.UrlMappings.FirstOrDefault(x => x.Code == code);
            if (mapping != null)
                return Redirect(mapping.OriginalUrl);
            return NotFound();
        }
    }
}
