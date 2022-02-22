using Microsoft.AspNetCore.Mvc;

namespace Web2.Controllers
{
    public class PlayerController : Controller
    {
        public IActionResult Players()
        {
            return View();
        }
    }
}
