using Newtonsoft.Json;
using System;
using System.Net;
using System.Web;
using System.Web.UI;

namespace TacklR.CacheManager.HttpHandlers
{
    //Auto check for .min.?
    internal abstract class PageHandler : Page, IHttpHandler//Do we want to use Page at all?
    {
        //Are these already in Page?
        internal HttpContext Context
        {
            get
            {
                return HttpContext.Current;
            }
        }

        internal HttpRequest Request
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }

        private string BodyContent { get; set; }
        private HttpStatusCode Status { get; set; }
        private TimeSpan? MaxAge { get; set; }
        private string BaseUrl { get; set; }

        internal PageHandler()
        {
            Status = HttpStatusCode.InternalServerError;
            BaseUrl = Configuration.BaseUrl;
        }

        internal IHttpHandler Content(string content, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? maxAge = null)
        {
            //Output cache?
            var _Layout = Resources.GetResourceString("_Layout.min.html");
            BodyContent = String.Format(_Layout, content, BaseUrl, Resources.BundleToken);
            Status = status;
            MaxAge = maxAge;//get better max-age, move this up to controller level so it's per-page?
            return this;
        }

        internal IHttpHandler View(string viewName, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? maxAge = null)
        {
            var _Layout = Resources.GetResourceString("_Layout.min.html");
            var body = Resources.GetResourceString(viewName);

            BodyContent = String.Format(_Layout, body, BaseUrl, Resources.BundleToken);
            Status = status;
            MaxAge = maxAge;//get better max-age, move this up to controller level so it's per-page?
            return this;
        }

        public override void ProcessRequest(HttpContext context)
        {
            context.Response.Headers.Override(Helpers.SecurityHeaders);

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetLastModified(Resources.BuildTime);
            context.Response.Cache.SetMaxAge(MaxAge ?? TimeSpan.FromDays(1));//Not working, why?

            base.ProcessRequest(context);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Response.StatusCode = (int)Status;
            writer.Write(BodyContent);
            base.Render(writer);
        }
    }
}