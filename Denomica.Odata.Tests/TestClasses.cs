using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.Odata.Tests
{
    public enum ContentStatus
    {
        Draft = 0,
        Published = 1,
        Archived = -1
    }

    public class Person
    {
        [Key]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        public DateOnly DateOfBirth { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }

    public class Employee : Person
    {
        [Key]
        public override string Id { get => base.Id; set => base.Id = value; }

        public DateOnly HireDate { get; set; }

        public Employee? Manager { get; set; }

        public Person? EmergencyContact { get; set; }
    }

    public class ContentItem
    {
        [Key]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        public string Title { get; set; } = string.Empty;

        public string? Body { get; set; }

        public ContentStatus Status { get; set; } = ContentStatus.Draft;
    }
}
