﻿using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Swartz.WarmupStarter
{
    public static class WarmupUtility
    {
        public static readonly string WarmupFilesPath = "~/App_Data/Warmup/";

        public static bool DoBeginRequest(HttpApplication httpApplication)
        {
            var url = ToUrlString(httpApplication.Request);
            var virtualFileCopy = EncodeUrl(url.Trim('/'));
            // ReSharper disable once AssignNullToNotNullAttribute
            var localCopy = Path.Combine(HostingEnvironment.MapPath(WarmupFilesPath), virtualFileCopy);

            if (File.Exists(localCopy))
            {
                httpApplication.Response.Cache.SetExpires(DateTime.Now.AddDays(-1));
                httpApplication.Response.Cache.SetValidUntilExpires(false);
                httpApplication.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                httpApplication.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                httpApplication.Response.Cache.SetNoStore();

                httpApplication.Response.WriteFile(localCopy);
                httpApplication.Response.End();
                return true;
            }

            if (File.Exists(httpApplication.Request.PhysicalPath))
            {
                return true;
            }

            return false;
        }

        public static string ToUrlString(HttpRequest request)
        {
            return $"{request.Url.Scheme}://{request.Headers["Host"]}{request.RawUrl}";
        }

        public static string EncodeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var sb = new StringBuilder();
            foreach (var c in url.ToLowerInvariant())
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("_");
                    foreach (var b in Encoding.UTF8.GetBytes(new[] {c}))
                    {
                        sb.Append(b.ToString("x"));
                    }
                }
            }

            return sb.ToString();
        }
    }
}