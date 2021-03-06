﻿using System.Net;
using System.Web;
using Tacklr.CacheManager.HttpHandlers;

namespace Tacklr.CacheManager.Controllers
{
    internal class ErrorController : PageHandler
    {
        internal IHttpHandler Error403()
        {
            return base.View("Error403.min.html", HttpStatusCode.Unauthorized);
        }

        internal IHttpHandler Error404()
        {
            return base.View("Error404.min.html", HttpStatusCode.NotFound);
        }

        internal IHttpHandler Error500()
        {
            return base.View("Error500.min.html", HttpStatusCode.InternalServerError);
        }
    }
}