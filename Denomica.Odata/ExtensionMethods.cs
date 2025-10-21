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

        /// <summary>
        /// Retrieves the last segment of the parsed OData URI path as a <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="parser">The <see cref="ODataUriParser"/> instance used to parse the OData URI.</param>
        /// <returns>The last segment of the parsed OData URI path as a <see cref="KeySegment"/>, or <see langword="null"/>  if
        /// the path is empty, the last segment is not a <see cref="KeySegment"/>, or the parser is unable to parse the
        /// path.</returns>
        public static KeySegment? GetKeySegment(this ODataUriParser parser)
        {
            var path = parser.ParsePath();
            if(null != path)
            {
                var keySegment = path.LastSegment as KeySegment;
                if(null != keySegment)
                {
                    return keySegment;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the identifiers of the selected paths from the specified <see cref="SelectExpandClause"/>.
        /// </summary>
        /// <remarks>This method iterates through the selected items in the <see
        /// cref="SelectExpandClause"/> and extracts the identifiers of paths that are explicitly selected. It skips
        /// items that are not of type <see cref="PathSelectItem"/>.</remarks>
        /// <param name="clause">The <see cref="SelectExpandClause"/> containing the selected items. Cannot be null.</param>
        /// <returns>An enumerable collection of strings representing the identifiers of the selected paths. If <paramref
        /// name="clause"/> is null or all items are selected, the collection will be empty.</returns>
        public static IEnumerable<string> SelectedPathIdentifiers(this SelectExpandClause clause)
        {
            if (null != clause && !clause.AllSelected)
            {
                foreach (var path in from x in clause.SelectedItems where x is PathSelectItem select (PathSelectItem)x)
                {
                    foreach (var selectedPath in path.SelectedPath)
                    {
                        yield return selectedPath.Identifier;
                    }
                }
            }

            yield break;
        }

        /// <summary>
        /// Converts the specified <see cref="OrderByClause"/> into a list of tuples, where each tuple contains a <see
        /// cref="SingleValuePropertyAccessNode"/> and its corresponding <see cref="OrderByDirection"/>.
        /// </summary>
        /// <remarks>The method traverses the <see cref="OrderByClause"/> chain, starting from the
        /// specified clause, and collects all segments into the resulting list. The list is ordered from the outermost
        /// clause to the innermost clause.</remarks>
        /// <param name="clause">The <see cref="OrderByClause"/> to convert. Must not be <c>null</c>.</param>
        /// <returns>A list of tuples, where each tuple represents an <see cref="OrderByClause"/> segment. The first item in the
        /// tuple is the <see cref="SingleValuePropertyAccessNode"/> representing the property being ordered, and the
        /// second item is the <see cref="OrderByDirection"/> indicating the sort direction.</returns>
        public static IList<Tuple<SingleValuePropertyAccessNode, OrderByDirection>> ToList(this OrderByClause clause)
        {
            var list = new List<Tuple<SingleValuePropertyAccessNode, OrderByDirection>>();

            var parent = clause;

            while (null != parent)
            {
                if (parent.Expression is SingleValuePropertyAccessNode)
                {
                    list.Add(new Tuple<SingleValuePropertyAccessNode, OrderByDirection>((SingleValuePropertyAccessNode)parent.Expression, parent.Direction));
                }

                parent = parent.ThenBy;
            }

            return list;
        }


        private static Uri MakeAbsolute(this Uri uri, string scheme = "odata", string host = "host")
        {
            var u = uri;
            if(!uri.IsAbsoluteUri)
            {
                var baseUri = new Uri($"{scheme}://{host}");
                u = new Uri(baseUri, uri);
            }

            return u;
        }
    }
}
