using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreIdentity.Controllers
{
    [Authorize] // Controller bazli kisitlama
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
