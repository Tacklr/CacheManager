using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tacklr.CacheManager.Models.Api;

namespace Tacklr.CacheManager.HttpHandlers
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

        internal HttpResponse Response
        {
            get
            {
                return HttpContext.Current.Response;
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
            try
            {
                Content = JsonConvert.SerializeObject(data);
                Status = status;
            }
            catch (Exception ex)
            {
                //log?
                Content = JsonConvert.SerializeObject(new JsonErrorViewModel { Success = false, Message = ex.Message });//better message?
                Status = HttpStatusCode.BadRequest;
            }
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
