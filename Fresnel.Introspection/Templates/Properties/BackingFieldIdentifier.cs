using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Envivo.Fresnel.Introspection.Configuration;
using System.Diagnostics;
using Envivo.Fresnel.Utils;

namespace Envivo.Fresnel.Introspection.Templates
{

    public class BackingFieldIdentifier
    {

        /// <summary>
        /// Assigns the backing fields for the properties using pseudo-fuzzy matching
        /// </summary>
        public FieldInfo GetBackingField(PropertyTemplate tProperty)
        {
            var tClass = tProperty.OuterClass;
            if (tClass.RealObjectType.IsInterface)
                return null;

            if (tProperty.BackingField != null)
                // This property has already been matched:
                return null;

            foreach (var field in tClass.Fields.Values)
            {
                var isMatch = IsAutoProperty(tProperty, field);

                if (!isMatch)
                {
                    isMatch = IsSimilarlyNamed(tProperty, field);
                }

                if (isMatch)
                {
                    if (CheckTypesMatch(tProperty, field))
                    {
                        // Got it:
                        return field;
                    }
                }
            }

            return null;
        }

        private bool IsAutoProperty(PropertyTemplate tProperty, FieldInfo field)
        {
            // Auto-property backing fields have the format "<prop_name>k__BackingField".
            // E.g. the auto-property "StartDate" would use the field "<StartDate>k__BackingField"
            var autoPropFieldName = string.Concat("<", tProperty.Name, ">k__BackingField");
            return field.Name.IsSameAs(autoPropFieldName);
        }

        private bool IsSimilarlyNamed(PropertyTemplate tProperty, FieldInfo field)
        {
            var fuzzyPropName = tProperty.Name.Replace("_", string.Empty);
            var fuzzyFieldName = field.Name.Replace("_", string.Empty);

            // Does the field name almost look like the property name?
            var isMatch = fuzzyFieldName.EndsWith(fuzzyPropName, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                // Are there only 1 or 2 differences between the names?
                var distance = fuzzyFieldName.CalculateDistanceFrom(fuzzyPropName);
                isMatch = (distance <= 2);
            }

            return isMatch;
        }

        private bool CheckTypesMatch(PropertyTemplate tProperty, FieldInfo field)
        {
            var fieldType = field.FieldType;

            // Do we have a nullable type?
            fieldType = fieldType.IsNullableType() ?
                        fieldType.GetGenericArguments()[0] :
                        fieldType;

            return fieldType.IsDerivedFrom(tProperty.PropertyType);
        }

        private void CheckNonAutoProperty(PropertyTemplate tProperty, FieldInfo field)
        {
            if (field == null &&
                tProperty.PropertyInfo.CanWrite &&
                tProperty.OuterClass.AssemblyReader.IsFrameworkAssembly == false)
            {
                // Hmm.. a Settable Property that doesn't have a Backing Field??
                Trace.TraceWarning(string.Concat("Cannot find the Backing Field for ", tProperty.FullName));
            }
        }

        ///// <summary>
        ///// Assigns the backing fields for the properties using pseudo-fuzzy matching
        ///// </summary>
        //public void AssignFieldsToProperties(ClassTemplate tClass)
        //{
        //    if (tClass.RealObjectType.IsInterface)
        //        return;

        //    foreach (var tProperty in tClass.Properties.Values)
        //    {
        //        if (tProperty.BackingField != null)
        //            // This property has already been matched:
        //            continue;

        //        var fuzzyPropName = tProperty.Name.Replace("_", string.Empty);

        //        // Auto-property backing fields have the format "<prop_name>k__BackingField".
        //        // E.g. the auto-property "StartDate" would use the field "<StartDate>k__BackingField"
        //        var autoPropFieldName = string.Concat("<", tProperty.Name, ">k__BackingField");

        //        foreach (var field in tClass.Fields.Values)
        //        {
        //            var fuzzyFieldName = field.Name.Replace("_", string.Empty);

        //            if (field.Name.IsSameAs(autoPropFieldName))
        //            {
        //                // This must be a C# 3.0 auto-property:
        //            }
        //            else if (!fuzzyFieldName.ToLower().EndsWith(fuzzyPropName.ToLower()))
        //            {
        //                // This isn't a candidate match:
        //                continue;
        //            }
        //            else
        //            {
        //                int distance = fuzzyFieldName.CalculateDistanceFrom(fuzzyPropName);
        //                if (distance > 2)
        //                {
        //                    // The difference beween the field and property name is too big:
        //                    continue;
        //                }
        //            }

        //            if (field.FieldType != tProperty.PropertyType)
        //            {
        //                // Types don't match:
        //                continue;
        //            }

        //            // Got it:
        //            tProperty.BackingField = field;
        //            break;
        //        }

        //    }
        //}

    }

}
