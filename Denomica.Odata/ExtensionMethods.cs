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

        /// <summary>
        /// Creates an <see cref="ODataUriParser"/> for the specified URI using the provided <see cref="IEdmModel"/>.
        /// </summary>
        /// <param name="model">
        /// The <see cref="IEdmModel"/> to use for parsing the URI. Cannot be <see langword="null"/>.
        /// </param>
        /// <param name="uri">
        /// The URI to parse, specified as a string. Can be either relative or absolute.
        /// </param>
        /// <returns>
        /// An <see cref="ODataUriParser"/> instance configured with the specified <paramref name="model"/> and
        /// <paramref name="uri"/>.
        /// </returns>
        public static ODataUriParser CreateUriParser(this IEdmModel model, string uri)
        {
            var u = new Uri(uri, UriKind.RelativeOrAbsolute);
            return model.CreateUriParser(u);
        }

        /// <summary>
        /// Creates an <see cref="ODataUriParser"/> for the specified <paramref name="uri"/> using the provided
        /// <paramref name="model"/>.
        /// </summary>
        /// <param name="model">
        /// The <see cref="IEdmModel"/> to use for parsing the OData URI.
        /// </param>
        /// <param name="uri">
        /// The absolute URI to be parsed. The URI must include a path with at least one segment.
        /// </param>
        /// <returns>
        /// An <see cref="ODataUriParser"/> instance configured to parse the specified <paramref name="uri"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="uri"/> does not specify a path with at least one segment.
        /// </exception>
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

        /// <summary>
        /// Creates an <see cref="ODataUriParser"/> instance for parsing the specified URI.
        /// </summary>
        /// <param name="builder">The <see cref="EdmModelBuilder"/> used to configure the OData model.</param>
        /// <param name="uri">The URI to be parsed. Can be either relative or absolute.</param>
        /// <returns>An <see cref="ODataUriParser"/> configured with the specified URI and the model from the <paramref
        /// name="builder"/>.</returns>
        public static ODataUriParser CreateUriParser(this EdmModelBuilder builder, string uri)
        {
            return builder.CreateUriParser(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Creates an <see cref="ODataUriParser"/> for the specified URI using the EDM model built by the <paramref
        /// name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="EdmModelBuilder"/> used to construct the EDM model for parsing the URI.</param>
        /// <param name="uri">The <see cref="Uri"/> to be parsed.</param>
        /// <returns>An <see cref="ODataUriParser"/> configured with the EDM model built by the <paramref name="builder"/>.</returns>
        public static ODataUriParser CreateUriParser(this EdmModelBuilder builder, Uri uri)
        {
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
