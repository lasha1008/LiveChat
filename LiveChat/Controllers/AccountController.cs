using LiveChat.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiveChat.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        TempData["ReturnUrl"] = HttpContext.Request.Headers["Referer"].ToString();
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginUser user)
    {
        if (ModelState.IsValid)
        {
            AuthorizeUser(user);
            var returnUrl = TempData["ReturnUrl"]?.ToString();
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index", "Home") : Redirect(returnUrl);
        }

        return View();
    }

    private void AuthorizeUser(LoginUser user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            // Additional authentication properties if needed
        };

        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

    }
}
