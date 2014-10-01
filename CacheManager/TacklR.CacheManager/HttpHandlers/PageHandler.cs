using Newtonsoft.Json;
using System;
using System.Net;
using System.Web;
using System.Web.UI;

namespace TacklR.CacheManager.HttpHandlers
{
    //Auto check for .min.?
    internal abstract class PageHandler : Page, IHttpHandler
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

        internal PageHandler()
        {
            Status = HttpStatusCode.InternalServerError;
        }

        internal IHttpHandler Content(string content, HttpStatusCode status = HttpStatusCode.OK)
        {
            //Output cache?
            var _Layout = Resources.GetResourceString("_Layout.min.html");
            BodyContent = String.Format(_Layout, content);
            Status = status;
            return this;
        }

        internal IHttpHandler View(string viewName, HttpStatusCode status = HttpStatusCode.OK)
        {
            var _Layout = Resources.GetResourceString("_Layout.min.html");
            var body = Resources.GetResourceString(viewName);

            BodyContent = String.Format(_Layout, body);
            Status = status;
            return this;
        }

        public override void ProcessRequest(HttpContext context)
        {
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