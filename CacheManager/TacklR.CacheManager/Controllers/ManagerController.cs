using System;
using System.Web;
using System.Web.Helpers;
using TacklR.CacheManager.HttpHandlers;

namespace TacklR.CacheManager.Controllers
{
    internal class ManagerController : PageHandler
    {
        internal IHttpHandler Index()
        {
            //String localization?
            return base.View("Index.min.html");
        }

        internal IHttpHandler VerificationToken()
        {
            var tokens = AntiForgeryHelpers.GetVerificationTokenContent(Request.Cookies);
            if (tokens.Item1 != default(HttpCookie))
            {
                tokens.Item1.Path = Configuration.BaseUrl;
                Response.SetCookie(tokens.Item1);
            }
            return base.Content(tokens.Item2, maxAge: TimeSpan.MinValue);
        }
    }
}