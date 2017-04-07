using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Praetorian.Proxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
#if DEBUG
                // TODO : host string here
                .UseUrls("http://*.praetorianprotect.localtest.me:5000")
#endif
                .Build();

            host.Run();
        }
    }
}
