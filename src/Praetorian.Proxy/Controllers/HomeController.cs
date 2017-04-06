using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Praetorian.Proxy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPraetorianProjectService praetorianProjectService;
        public HomeController(IPraetorianProjectService praetorianProjectService)
        {
            this.praetorianProjectService = praetorianProjectService;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await praetorianProjectService.GetAllProjects();
            return View(projects);
        }

        [HttpGet("_praetorian/{clientName}/{projectName}")]
        public async Task<IActionResult> Index(string clientName, string projectName)
        {
            var project = await praetorianProjectService.GetProject(clientName, projectName);
            if (project == null)
            {
                return NotFound();
            }

            var token = praetorianProjectService.GenerateSiteReferenceToken(project);
            HttpContext.Response.Cookies.AddPraetorianSiteCookie(token);

            return Redirect("/");
        }
    }
}
