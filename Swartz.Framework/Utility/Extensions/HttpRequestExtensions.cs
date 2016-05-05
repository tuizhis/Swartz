using System;
using System.Web;

namespace Swartz.Utility.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        ///     Returns the root part of a request.
        /// </summary>
        /// <example>http://localhost:3030</example>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToRootUrlString(this HttpRequestBase request)
        {
            return $"{request.Url.Scheme}://{request.Headers["Host"]}";
        }

        /// <summary>
        ///     Returns the root part of a request.
        /// </summary>
        /// <example>http://localhost:3030</example>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToRootUrlString(this HttpRequest request)
        {
            return $"{request.Url.Scheme}://{request.Headers["Host"]}";
        }

        /// <summary>
        ///     Returns the application root part of a request.
        /// </summary>
        /// <example>http://localhost:3030/OrchardLocal</example>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToApplicationRootUrlString(this HttpRequestBase request)
        {
            var url = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Headers["Host"],
                request.ApplicationPath == "/" ? string.Empty : request.ApplicationPath);
            return url;
        }

        /// <summary>
        ///     Returns the application root part of a request.
        /// </summary>
        /// <example>http://localhost:3030/OrchardLocal</example>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToApplicationRootUrlString(this HttpRequest request)
        {
            var url =
                $"{request.Url.Scheme}://{request.Headers["Host"]}{(request.ApplicationPath == "/" ? string.Empty : request.ApplicationPath)}";
            return url;
        }

        /// <summary>
        ///     Returns the client requested url.
        /// </summary>
        /// <example>http://localhost:3030/OrchardLocal/Admin/Blogs</example>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToUrlString(this HttpRequestBase request)
        {
            return $"{request.Url.Scheme}://{request.Headers["Host"]}{request.RawUrl}";
        }

        /// <summary>
        ///     Returns the client requested url.
        /// </summary>
        /// <example>http://localhost:3030/OrchardLocal/Admin/Blogs</example>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToUrlString(this HttpRequest request)
        {
            return $"{request.Url.Scheme}://{request.Headers["Host"]}{request.RawUrl}";
        }


        /// <summary>
        ///     Returns wether the specified url is local to the host or not
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsLocalUrl(this HttpRequestBase request, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (url.StartsWith("~/"))
            {
                return true;
            }

            if (url.StartsWith("//") || url.StartsWith("/\\"))
            {
                return false;
            }

            // at this point is the url starts with "/" it is local
            if (url.StartsWith("/"))
            {
                return true;
            }

            // at this point, check for an fully qualified url
            try
            {
                var uri = new Uri(url);
                if (uri.Authority.Equals(request.Headers["Host"], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                // mall-formed url e.g, "abcdef"
                return false;
            }
        }
    }
}