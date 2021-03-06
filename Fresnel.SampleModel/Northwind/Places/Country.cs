using System;
using System.ComponentModel.DataAnnotations;

namespace Envivo.Fresnel.SampleModel.Northwind.Places
{
    public class Country
    {
        [Key]
        public Guid ID { get; set; }

        [ConcurrencyCheck]
        public long Version { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}