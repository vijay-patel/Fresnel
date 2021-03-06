using Envivo.Fresnel.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Envivo.Fresnel.SampleModel.Northwind.People
{
    public class Customer : Role
    {
        [Relationship(Type = RelationshipType.Has)]
        public BaseParty Party { get; set; }

        [Relationship(Type = RelationshipType.Owns)]
        public ContactDetails Contact { get; set; }

        public override string ToString()
        {
            return typeof(Customer).Name;
        }
    }
}