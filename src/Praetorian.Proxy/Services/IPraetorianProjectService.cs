using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Praetorian.Proxy.Controllers
{
    public interface IPraetorianProjectService
    {
        Task<PraetorianProject> GetProject(string clientName, string projectName);
        string GenerateSiteReferenceToken(PraetorianProject project);

        Task<PraetorianProject> GetProjectFromSiteReferenceToken(string token);
        Task<IEnumerable<PraetorianProject>> GetAllProjects();
    }
}