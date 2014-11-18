using System;
using Envivo.Fresnel.DomainTypes.Interfaces;
using Envivo.Fresnel.DomainTypes;

namespace Envivo.Fresnel.Introspection.Templates
{

    /// <summary>
    /// Determines if the given class has an IAudit property
    /// Typically used to method audit information when persisting Domain Objects
    /// </summary>
    public class IsObjectAuditableSpecification : ISpecification<Type>
    {
        private readonly Type _IAuditType = typeof(IAudit);

        public IAssertion IsSatisfiedBy(Type sender)
        {
            foreach (var prop in sender.GetProperties())
            {
                if (prop.PropertyType == _IAuditType)
                {
                    return Assertion.Pass();
                }
            }

            var msg = string.Concat(sender.GetType().Name, " does not have a property of type IAudit");
            return Assertion.FailWithWarning(msg);
        }

    }

}