using System;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Tacklr.CacheManager.Models.Api;

namespace Tacklr.CacheManager.HttpHandlers
{
    internal abstract class DataHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

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