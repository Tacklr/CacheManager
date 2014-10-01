using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
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
    }
}
