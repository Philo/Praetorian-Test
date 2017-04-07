using Microsoft.AspNetCore.Http;
using Praetorian.Proxy.Domain;

namespace Praetorian.Proxy.Extensions
{
    internal static class HttpCookiesExtensions
    {
        //internal static void AddPraetorianSiteCookie(this IResponseCookies cookies, string token, PraetorianProject project)
        //{
        //    // TODO : host string here
        //    cookies.Append("__praetorian_siteref", token, new CookieOptions()
        //    {
        //        Domain = ".praetorianprotect.localtest.me"
        //    });
        //}

        //internal static string GetPraetorianSiteCookieToken(this IRequestCookieCollection cookies)
        //{
        //    if (cookies.ContainsKey("__praetorian_siteref"))
        //    {
        //        return cookies["__praetorian_siteref"];
        //    }
        //    return null;
        //}
    }
}