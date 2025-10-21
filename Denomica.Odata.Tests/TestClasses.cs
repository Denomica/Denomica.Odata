using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.Odata.Tests
{

    public class Person
    {
        [Key]
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class Employee : Person
    {
        public DateOnly HireDate { get; set; }
    }
}
