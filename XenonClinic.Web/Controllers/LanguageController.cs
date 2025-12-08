using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace XenonClinic.Web.Controllers;

public class LanguageController : Controller
{
    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return BadRequest("Culture cannot be empty");
        }

        // Validate culture is supported
        if (culture != "en" && culture != "ar")
        {
            return BadRequest("Unsupported culture");
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            }
        );

        return LocalRedirect(returnUrl ?? "/");
    }
}
