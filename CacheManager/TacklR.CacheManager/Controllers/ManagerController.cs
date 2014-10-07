using System.Web;
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