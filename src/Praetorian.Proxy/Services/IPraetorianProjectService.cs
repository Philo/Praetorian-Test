using System.Collections.Generic;
using System.Threading.Tasks;
using Praetorian.Proxy.Domain;

namespace Praetorian.Proxy.Services
{
    public interface IPraetorianProjectService
    {
        Task<PraetorianProject> GetProject(string clientName, string projectName);
        string GenerateSiteReferenceToken(PraetorianProject project);

        Task<PraetorianProject> GetProjectFromSiteReferenceToken(string token);
        Task<IEnumerable<PraetorianProject>> GetAllProjects();
    }
}