using Denomica.OData;

namespace Denomica.Odata.Tests
{
    [TestClass]
    public sealed class OdataTests
    {
        [TestMethod]
        public void CreateEdmBuilder01()
        {
            var builder = new EdmModelBuilder()
                .Build();

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        public void CreateEdmBuilder02()
        {
            var builder = new EdmModelBuilder()
                .AddEntity<Person>()
                .Build();

            Assert.IsNotNull(builder);
        }



        [TestMethod]
        public void CreateUriParser01()
        {
            var parser = new EdmModelBuilder()
                .AddEntity<Person>()
                .Build()
                .CreateUriParser("/persons");

            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void CreateUriParser02()
        {
            var parser = new EdmModelBuilder()
                .AddEntity<Person>()
                .Build()
                .CreateUriParser("https://api.company.com/people");

            var uri = parser.ParseUri();
            Assert.IsNotNull(uri);
        }

        [TestMethod]
        [Description("Creates a URI parser that filters on a property defined on a specified entity type.")]
        public void CreateUriParser03()
        {
            var parser = new EdmModelBuilder()
                .AddEntity<Employee>()
                .CreateUriParser("https://api.company.com/employees?$filter=hireDate gt 2020-01-01");
            var uri = parser.ParseUri();

            Assert.IsNotNull(uri);
        }

        [TestMethod]
        [Description("Creates a URI parser that filters on a property defined on the base class for a given entity type.")]
        public void CreateUriParser04()
        {
            var parser = new EdmModelBuilder()
                .AddEntity<Employee>()
                .CreateUriParser("https://company.com/api/employees?$filter=lastName eq 'Smith'");
            var uri = parser.ParseUri();

            Assert.IsNotNull(uri);
        }
    }
}
