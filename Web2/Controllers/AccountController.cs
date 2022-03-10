using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web2.Helpers;
using Web2.Models;

namespace Web2.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserHelper UserManager;
        private readonly Api api = new Api();
        public AccountController(UserHelper _UserManager)
        {
            UserManager = _UserManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View("Login", new User()
            {
                EmailAddress = "",
            });
        }
        public async Task<IActionResult> LoginUser(string email, string password)
        {
            var res = await UserManager.Login(email, password);
            //SignInManager.SignInAsync
            if (res.EmailAddress == "Invalid")
            {
                return View("~/Views/Account/Login.cshtml", res);
            }
            if (await UserManager.GetLoginCount() == 1 && !(await api.IsUserInOrg(UserManager.GetUserId())))
            {
                return View("~/Views/Organisation/CreateOrganisation.cshtml");
            }
            return View("~/Views/Home/Home.cshtml");
        }
        public async Task<IActionResult> Regrister()
        {
            return View("Regrister");
        }
        public async Task<IActionResult> RegristerUser(string email, string username, string password)
        {
            await UserManager.Regrister(new Models.User()
            {
                EmailAddress = email,
                Username = username,
                Password = password
            });
            return View("~/Views/Home/Home.cshtml");
        }
        public async Task<IActionResult> ForgotPassword()
        {
            return View("ForgotPassword");
        }
        public async Task<IActionResult> Forgor(string email)
        {
            string res = await UserManager.ForgotPassword(email);
            if (res == "Email not found" || res == "There is already an active token")
            {
                return View("~/Views/Home/Error.cshtml", new Donkey()
                {
                    Value = res
                });
            }
            return View("~/Views/Home/Home.cshtml");
        }
        public async Task<IActionResult> ResetPassword(Guid Id)
        {
            AccountRecovery reco = new AccountRecovery()
            {
                Id = Id
            };
            return View("ResetPassword", reco);
        }
        public async Task<IActionResult> UserResetPassword(string Id, string newpassword)
        {
            string res = await UserManager.ResetPassword(Id, newpassword);
            return View("~/Views/Home/Home.cshtml");
        }
        public IActionResult Logout()
        {
            UserManager.Logout();
            return View("~/Views/Home/Landing.cshtml", new Donkey()
            {
                Value = "You have been logged out"
            });
        }
    }
}
