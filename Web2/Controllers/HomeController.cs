using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web2.Helpers;
using Web2.Models;

namespace Web2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserHelper UserManager;

        public HomeController(ILogger<HomeController> logger, UserHelper _UserManager)
        {
            _logger = logger;
            UserManager = _UserManager;
        }

        public async Task<IActionResult> Home()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Landing(Donkey donkey)
        {
            return View("Landing", donkey);
        }
    }
}