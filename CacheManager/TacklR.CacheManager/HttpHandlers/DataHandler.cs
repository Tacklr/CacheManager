using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TacklR.CacheManager.HttpHandlers
{
    internal abstract class DataHandler : IHttpHandler
    {
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string Content { get; set; }
        private HttpStatusCode Status { get; set; }

        public IHttpHandler Json(object data, HttpStatusCode status = HttpStatusCode.OK)//serializer option? xml?
        {
            Content = JsonConvert.SerializeObject(data);
            Status = status;
            return this;
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();

            context.Response.Headers.Override(Helpers.SecurityHeaders);

            context.Response.StatusCode = (int)Status;
            context.Response.ContentType = "application/json";
            context.Response.Write(Content);
            context.Response.End();
        }
    }
}
