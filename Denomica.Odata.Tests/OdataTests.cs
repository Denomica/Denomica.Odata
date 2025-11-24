using Denomica.OData;
using Humanizer;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Denomica.Odata.Tests
{
    [TestClass]
    public sealed class OdataTests
    {

        [TestMethod]
        public void BuildModel01()
        {
            var model = new EdmModelBuilder()
                .AddEntity<Person>()
                .Build();

            var personType = (EdmEntityType)model.SchemaElements.First(x => x.FullName() == typeof(Person).FullName);
            Assert.IsNotNull(personType);

            var dobProperty = (EdmStructuralProperty)personType.DeclaredProperties.First(x => x.Name == nameof(Person.DateOfBirth).Camelize());
            Assert.IsFalse(dobProperty.Type.IsNullable, "The DateOfBirth property must not be nullable.");
        }

        [TestMethod]
        public void BuildModel02()
        {
            var model = new EdmModelBuilder()
                .AddEntity(typeof(Person))
                .AddEntity(typeof(Employee))
                .Build();

            var personType = (EdmEntityType)model.FindDeclaredType(typeof(Person).FullName);
            Assert.IsNotNull(personType);

            var employeeType = (EdmEntityType)model.FindDeclaredType(typeof(Employee).FullName);
            Assert.IsNotNull(employeeType);
            Assert.AreEqual(personType, employeeType.BaseEntityType());
        }

        [TestMethod]
        public void BuildModel03()
        {
            var model = new EdmModelBuilder()
                .AddEntity(typeof(Employee))
                .AddEntity(typeof(Person))
                .Build();
        }

        [TestMethod]
        public void BuildModel04()
        {
            var model = new EdmModelBuilder()
                .AddEntity<ContentItem>()
                .Build();

            var ciElement = (EdmEntityType)model.SchemaElements.First(x => x.Name == nameof(ContentItem));

            string[] properties = ["id", "title", "body", "status"];
            foreach(var p in properties)
            {
                var prop = ciElement.DeclaredProperties.FirstOrDefault(x => x.Name == p);
                Assert.IsNotNull(prop, $"The property '{p}' must be contained in the defined model.");
            }
        }



        [TestMethod]
        [Description("Creates an empty model builder.")]
        public void CreateEdmBuilder01()
        {
            var builder = new EdmModelBuilder()
                .Build();

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [Description("Creates a model builder with a single entity type.")]
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

        [TestMethod]
        [Description("Creates a URI parser that filters on a date property.")]
        public void CreateUriParser05()
        {
            var model = new EdmModelBuilder()
                .AddEntity<Person>()
                .SetEntitySet<Person>("persons")
                .AddEntityKey<Person>(nameof(Person.Id))
                .Build();

            var parser = model.CreateUriParser("/persons?$filter=dateOfBirth lt 1980-01-01");
            Assert.IsNotNull(parser);

            var filter = parser.ParseFilter();
            Assert.IsNotNull(filter);

            var count = parser.ParseCount();
            Assert.IsNull(count);
        }

        [TestMethod]
        [Description("Creates a URI parser that filters on date properties from both the base and derived entity types.")]
        public void CreateUriParser06()
        {
            var model = new EdmModelBuilder()
                .AddEntity<Person>()
                .AddEntity<Employee>()
                .AddEntityKey<Employee>(nameof(Employee.Id))
                .SetEntitySet<Employee>("employees")
                .Build();

            var parser = model.CreateUriParser("/employees?$filter=hireDate gt 2000-01-01 and dateOfBirth gt 1980-01-01");
            Assert.IsNotNull(parser);

            var filter = parser.ParseFilter();
            Assert.IsNotNull(filter?.Expression);

            var orderBy = parser.ParseOrderBy();
            Assert.IsNull(orderBy);
        }

        [TestMethod]
        [Description("Creates a URI parser that retrieves an entity by ID.")]
        public void CreateUriParser07()
        {
            var parser = new EdmModelBuilder()
                .AddEntity<Person>()
                .AddEntity<Employee>()
                .CreateUriParser("/employees?$id='007'");

            Assert.IsNotNull(parser);
            var entityId = parser.ParseEntityId();
            Assert.IsNotNull(entityId);
        }

        [TestMethod]
        [ExpectedException(typeof(ODataUnrecognizedPathException))]
        [Description("Creates a URI parser with an invalid path and expects an exception.")]
        public void CreateUriParser08()
        {
            var parser = new EdmModelBuilder()
                .AddEntity<Person>()
                .AddEntity<Employee>()
                .CreateUriParser("/foo");

            var filter = parser.ParseFilter();
        }

        [TestMethod]
        [Description("Creates a URI parser that retrieves an employee by key.")]
        public void CreateUriParser09()
        {
            var parser = new EdmModelBuilder()
                .AddEntity<Person>()
                .AddEntity<Employee>()
                .CreateUriParser("https://api.company.com/employees('007')");
            Assert.IsNotNull(parser);

            var keySegment = parser.GetKeySegment();
            Assert.IsNotNull(keySegment);

            var key = keySegment.Keys.Single();
            Assert.AreEqual("id", key.Key);
            Assert.AreEqual("007", key.Value);
        }
    }
}
