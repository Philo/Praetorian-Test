using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Praetorian.Proxy.Extensions;
using Praetorian.Proxy.Services;

namespace Praetorian.Proxy.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = "praetorian")]
    public class LogoutController : Controller
    {
        public async Task<IActionResult> Index()
        {
            await HttpContext.Authentication.SignOutAsync("praetorian");
            return RedirectToAction("Index", "Home");
        }
    }

    public class LoginController : Controller
    {
        public class LoginModel
        {
            [Display(Name = "Email address")]
            [Required]
            [DataType(DataType.EmailAddress)]
            [EmailAddress]
            public string Email { get; set; }
            [Display(Name = "Password")]
            [Required]
            [MinLength(8)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public IActionResult Denied()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model, string returnUrl)
        {
            var i = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, model.Email)
            }, "praetorian");
            var p = new ClaimsPrincipal(i);

            await HttpContext.Authentication.SignInAsync("praetorian", p);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(returnUrl);
        }
    }

    [Authorize(ActiveAuthenticationSchemes = "praetorian")]
    public class ProjectsController : Controller
    {
        private readonly IPraetorianProjectService praetorianProjectService;
        public ProjectsController(IPraetorianProjectService praetorianProjectService)
        {
            this.praetorianProjectService = praetorianProjectService;
        }


        public async Task<IActionResult> Index()
        {
            var projects = await praetorianProjectService.GetAllProjects();
            return View(projects);
        }

        //[HttpGet("{clientName}/{projectName}")]
        //public async Task<IActionResult> Index(string clientName, string projectName)
        //{
        //    var project = await praetorianProjectService.GetProject(clientName, projectName);
        //    if (project == null)
        //    {
        //        return NotFound();
        //    }

        //    return Redirect(project.BuildProjectUri(HttpContext));
        //}
    }

    
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }


    }
}
