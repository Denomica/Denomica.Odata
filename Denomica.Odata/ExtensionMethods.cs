using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Denomica.OData
{
    /// <summary>
    /// Exposes extension methods for working with OData and Edm models.
    /// </summary>
    public static class ExtensionMethods
    {

        public static Uri AppendFilter(this Uri uri, string filterExpression)
        {
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            var filter = parameters["$filter"];
            if(filter?.Length > 0)
            {
                parameters["$filter"] = $"{filter} and {filterExpression}";
            }
            else
            {
                parameters["$filter"] = filterExpression;
            }

            var uriBuilder = new StringBuilder(uri.GetLeftPart(UriPartial.Path)).Append('?');
            int keyCount = 0;
            foreach(var key in parameters.AllKeys)
            {
                if (keyCount > 0) uriBuilder.Append('&');

                uriBuilder.Append(key)
                    .Append("=")
                    .Append(parameters[key]);

                keyCount++;
            }

            return new Uri(uriBuilder.ToString());
        }

        public static Uri AppendFilterIf(this Uri uri, string filterExpression, bool condition)
        {
            if(condition)
            {
                uri = uri.AppendFilter(filterExpression);
            }

            return uri;
        }

        public static ODataUriParser CreateUriParser(this IEdmModel model, string uri)
        {
            var u = new Uri(uri, UriKind.RelativeOrAbsolute);
            return model.CreateUriParser(u);
        }

        public static ODataUriParser CreateUriParser(this IEdmModel model, Uri uri)
        {
            var u = uri.MakeAbsolute();

            if(u.LocalPath.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries).Length == 0)
            {
                throw new ArgumentException("The given URI must specify a path with at least one segment.", nameof(u));
            }

            var pathOnlyUri = u.GetLeftPart(UriPartial.Path).ToString();
            var rootUri = new Uri(pathOnlyUri.Substring(0, pathOnlyUri.LastIndexOf('/')));

            return new ODataUriParser(model, rootUri, u);
        }

        public static ODataUriParser CreateUriParser<TEntity>(this EdmModelBuilder builder, string uri)
        {
            return builder.CreateUriParser<TEntity>(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        public static ODataUriParser CreateUriParser<TEntity>(this EdmModelBuilder builder, Uri uri)
        {
            var u = uri.MakeAbsolute();
            var path = u.OriginalString.Contains('?') ? u.OriginalString.Substring(0, u.OriginalString.IndexOf('?')) : u.OriginalString;
            var segments = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if(segments.Length > 1)
            {
                builder.AddUriSegment<TEntity>(segments.Last());
            }

            return builder
                .Build()
                .CreateUriParser(uri);
        }



        private static Uri MakeAbsolute(this Uri uri, string scheme = "odata", string host = "host")
        {
            var u = uri;
            if(!uri.IsAbsoluteUri)
            {
                var pathPrefix = uri.OriginalString.StartsWith("/") ? "/" : "";
                u = new Uri($"{scheme}://{host}{pathPrefix}{uri.OriginalString}");
            }

            return u;
        }
    }
}
