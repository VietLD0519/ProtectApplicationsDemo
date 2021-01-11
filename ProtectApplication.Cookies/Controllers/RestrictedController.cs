using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;

namespace ProtectApplication.Cookies.Controllers
{
    public class RestrictedController : Controller
    {
        private readonly IDataProtectionProvider _protector;
        private readonly CookieAuthenticationOptions _opts;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RestrictedController(IDataProtectionProvider protector, IOptionsMonitor<CookieAuthenticationOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _protector = protector;
            _opts = options.Get("DemoProtectApplication.Cookies");
            _httpContextAccessor = httpContextAccessor;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Managers")]
        public IActionResult RoleBased()
        {
            return View();
        }

        [Authorize(Policy = "ManagerOnly")]
        public IActionResult ClaimBased()
        {
            return View();
        }

        [Authorize(Policy = "ManagerFromSalesDepartment")]
        public IActionResult PolicyBased()
        {
            return View();
        }

        [Authorize]
        public IActionResult DecryptCookie()
        {
            var cookieManager = new ChunkingCookieManager();
            var cookie = cookieManager.GetRequestCookie(HttpContext, ".AspNetCore.Identity.Application");

            var dataProtector = _protector.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", "Identity.Application", "v2");

            //Get the decrypted cookie as plain text
            UTF8Encoding specialUtf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            var protectedBytes = Base64UrlTextEncoder.Decode(cookie);
            
            var plainBytes = dataProtector.Unprotect(protectedBytes);
            var plainText = specialUtf8Encoding.GetString(plainBytes);


            //Get teh decrypted cookies as a Authentication Ticket
            var ticketDataFormat = new TicketDataFormat(dataProtector);
            var ticket = ticketDataFormat.Unprotect(cookie);

            return View(new CookieDetails(plainText, ticket));
        }

    }

    public class CookieDetails
    {
        public string PlainText { get; }
        public AuthenticationTicket Ticket { get; }

        public CookieDetails(string plainText, AuthenticationTicket ticket)
        {
            PlainText = plainText;
            Ticket = ticket;
        }
    }
}
