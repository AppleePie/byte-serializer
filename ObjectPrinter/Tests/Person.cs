using System;
using System.Collections.Generic;

namespace ObjectPrinter.Tests
{
    public class Person
    {
        public Guid Id;
        public List<Person> Family;
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime BirthDay { get; set; }
        public Person Spouse { get; set; }
    }
}