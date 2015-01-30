using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Tacklr.CacheManager.HttpHandlers
{
    //Auto check for .min.?
    internal static class MimeType
    {
        internal static string Css = "text/css";
        internal static string Default = "text/plain";
        internal static string Eot = "application/vnd.ms-fontobject";
        internal static string Js = "text/javascript";
        internal static string Json = "application/json";
        internal static string Otf = "font/opentype";
        internal static string Svg = "image/svg+xml";
        internal static string Ttf = "application/octet-stream";
        internal static string Woff = "application/font-woff";
        internal static string Woff2 = "application/font-woff2";
        private static Dictionary<string, string> ExtensionMap = new Dictionary<string, string>();

        static MimeType()
        {
            ExtensionMap.Add(".css", MimeType.Css);
            ExtensionMap.Add(".eot", MimeType.Eot);
            ExtensionMap.Add(".js", MimeType.Js);
            ExtensionMap.Add(".json", MimeType.Json);
            ExtensionMap.Add(".otf", MimeType.Otf);
            ExtensionMap.Add(".svg", MimeType.Svg);
            ExtensionMap.Add(".ttf", MimeType.Ttf);
            ExtensionMap.Add(".woff", MimeType.Woff);
            ExtensionMap.Add(".woff2", MimeType.Woff2);
        }

        internal static bool TryFromExtension(string extension, out string mimeType)
        {
            mimeType = default(string);
            extension = extension.ToLowerInvariant();
            return ExtensionMap.TryGetValue(extension, out mimeType);
        }
    }

    internal class ResourceHandler : IHttpHandler
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

        private string ContentType { get; set; }
        private TimeSpan? MaxAge { get; set; }
        private string Name { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            if (!Resources.ResourceExists(Name))
                throw new Exception();//how can I get back to the 404 handler?

            context.Response.Clear();

            context.Response.Headers.Override(Helpers.SecurityHeaders);

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetLastModified(Resources.BuildTime);
            context.Response.Cache.SetMaxAge(MaxAge ?? TimeSpan.FromDays(365));

            context.Response.ContentType = ContentType;
            context.Response.BinaryWrite(Resources.GetResourceBytes(Name));
            context.Response.End();
        }

        internal IHttpHandler Resource(string name, string contentType = null, TimeSpan? maxAge = null)
        {
            if (String.IsNullOrEmpty(contentType) && !MimeType.TryFromExtension(Path.GetExtension(name), out contentType))
                contentType = MimeType.Default;

            ContentType = contentType;
            MaxAge = maxAge;
            Name = name;
            return this;
        }
    }
}