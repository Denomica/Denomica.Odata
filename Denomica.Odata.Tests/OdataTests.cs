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
                .CreateUriParser("https://api.company.com/persons");

            var uri = parser.ParseUri();
        }
    }
}
