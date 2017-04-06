using Microsoft.AspNetCore.Http;

namespace Praetorian.Proxy.Controllers
{
    internal static class HttpCookiesExtensions
    {
        internal static void AddPraetorianSiteCookie(this IResponseCookies cookies, string token)
        {
            cookies.Append("__praetorian_siteref", token);
        }

        internal static string GetPraetorianSiteCookieToken(this IRequestCookieCollection cookies)
        {
            if (cookies.ContainsKey("__praetorian_siteref"))
            {
                return cookies["__praetorian_siteref"];
            }
            return null;
        }
    }
}