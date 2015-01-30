using System;
using System.Net;
using System.Web;
using System.Web.UI;

namespace Tacklr.CacheManager.HttpHandlers
{
    //Auto check for .min.?
    //Refactor this.
    internal abstract class PageHandler : Page, IHttpHandler//Do we want to use Page at all?
    {
        internal PageHandler()
        {
            Status = HttpStatusCode.InternalServerError;
            BaseUrl = Configuration.BaseUrl;
        }

        //Are these already in Page? I get an out of context exception when I use the Page versions.
        new internal HttpContext Context
        {
            get
            {
                return HttpContext.Current;
            }
        }

        new internal HttpRequest Request
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }

        new internal HttpResponse Response
        {
            get
            {
                return HttpContext.Current.Response;
            }
        }

        private string BaseUrl { get; set; }
        private string BodyContent { get; set; }
        private TimeSpan? MaxAge { get; set; }
        private HttpStatusCode Status { get; set; }

        public override void ProcessRequest(HttpContext context)
        {
            context.Response.Headers.Override(Helpers.SecurityHeaders);

            if (MaxAge == TimeSpan.MinValue)//better way to indicate no cache?
            {
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Cache.AppendCacheExtension("no-store, must-revalidate");
                context.Response.AppendHeader("Pragma", "no-cache");
                context.Response.AppendHeader("Expires", "-1");
            }
            else
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetLastModified(Resources.BuildTime);
                context.Response.Cache.SetMaxAge(MaxAge ?? TimeSpan.FromDays(1));//Not working, why?
            }

            base.ProcessRequest(context);
        }

        internal IHttpHandler Content(string content, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? maxAge = null)
        {
            //Output cache?
            BodyContent = content;
            Status = status;
            MaxAge = maxAge;//get better max-age, move this up to controller level so it's per-page?
            return this;
        }

        internal IHttpHandler View(string viewName, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? maxAge = null)
        {
            var _Layout = Resources.GetResourceString("_Layout.min.html");
            var body = Resources.GetResourceString(viewName);

            BodyContent = String.Format(_Layout, body, BaseUrl, Resources.CssBundleToken, Resources.JsBundleToken);
            Status = status;
            MaxAge = maxAge;//get better max-age, move this up to controller level so it's per-page?
            return this;
        }

        internal IHttpHandler ViewContent(string content, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? maxAge = null)
        {
            //Output cache?
            var _Layout = Resources.GetResourceString("_Layout.min.html");
            BodyContent = String.Format(_Layout, content, BaseUrl, Resources.CssBundleToken, Resources.JsBundleToken);
            Status = status;
            MaxAge = maxAge;//get better max-age, move this up to controller level so it's per-page?
            return this;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Response.StatusCode = (int)Status;
            writer.Write(BodyContent);
            base.Render(writer);
        }
    }
}