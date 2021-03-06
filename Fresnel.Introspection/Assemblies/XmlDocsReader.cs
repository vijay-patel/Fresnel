using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Envivo.Fresnel.Introspection.Assemblies
{
    /// <summary>
    /// Reads the comments from the Domain Assembly XML document, and inserts them into the appropriate Templates
    /// </summary>
    /// <remarks>
    /// See the structure of the XML documentation for a better understanding.
    /// </remarks>
    public class XmlDocsReader
    {
        private readonly string XPATH_ROOT = "//members/member ";

        private XPathNavigator _XPathNavigator;
        private AssemblyReader _AssemblyReader;

        private XmlComments _EmptyXmlComments = new XmlComments();

        public void InitialiseFrom(AssemblyReader assemblyReader)
        {
            _AssemblyReader = assemblyReader;
            this.LoadXmlCommentsFor(assemblyReader.Assembly);
        }

        /// <summary>
        /// Returns True if the XML comments file is available
        /// </summary>
        public bool IsHelpAvailable
        {
            get { return (_XPathNavigator != null); }
        }

        public XmlComments GetXmlCommentsFor(ClassTemplate tClass)
        {
            if (this.IsHelpAvailable == false)
                return _EmptyXmlComments;

            var xmlComments = this.GetXmlComments(tClass);

            // If the summary is empty, we need to obtain the value from one of the classes up the inheritance chain:
            var superType = tClass.RealType.BaseType;
            while ((superType != null) && xmlComments.Summary.IsEmpty())
            {
                try
                {
                    // Base generic types don't return a FullName:
                    if (superType.FullName.IsEmpty())
                        return _EmptyXmlComments;

                    // NB: If we can't find the super class, there's no point in continuing:
                    var tSuperClass = _AssemblyReader.GetTemplate(superType) as ClassTemplate;
                    if (tSuperClass == null)
                        return _EmptyXmlComments;

                    xmlComments = tSuperClass.XmlComments;
                }
                finally
                {
                    superType = superType.BaseType;
                }
            }

            return xmlComments;
        }

        public XmlComments GetXmlCommentsFor(PropertyTemplate tProperty)
        {
            if (this.IsHelpAvailable == false)
                return _EmptyXmlComments;

            //if (tProperty.IsFrameworkMember)
            //    return _EmptyXmlComments;

            var xmlComments = this.GetXmlComments(tProperty);

            try
            {
                // If the summary is empty, we need to obtain the value from one of the classes up the inheritance chain:
                var superType = tProperty.OuterClass.RealType.BaseType;
                while ((superType != null) && xmlComments.Summary.IsEmpty())
                {
                    try
                    {
                        // Base generic types don't return a FullName:
                        if (superType.FullName.IsEmpty())
                            return _EmptyXmlComments;

                        // NB: If we can't find the super class, there's no point in continuing:
                        var tSuperClass = _AssemblyReader.GetTemplate(superType) as ClassTemplate;
                        if (tSuperClass == null)
                            return _EmptyXmlComments;

                        var tSuperProperty = tSuperClass.Properties.TryGetValueOrNull(tProperty.Name);
                        if (tSuperProperty == null)
                            return _EmptyXmlComments;

                        xmlComments = tSuperProperty.XmlComments;
                    }
                    finally
                    {
                        superType = superType.BaseType;
                    }
                }
            }
            finally
            {
                if (xmlComments.Summary.IsEmpty() && tProperty.IsDomainObject)
                {
                    // Use the comments from the inner class:
                    var tInnerClass = _AssemblyReader.GetTemplate(tProperty.PropertyType) as ClassTemplate;
                    if (tInnerClass != null)
                    {
                        xmlComments = tInnerClass.XmlComments;
                    }
                }
            }

            return xmlComments;
        }

        public XmlComments GetXmlCommentsFor(MethodTemplate tMethod)
        {
            if (this.IsHelpAvailable == false)
                return _EmptyXmlComments;

            if (tMethod.IsVisible == false)
                return _EmptyXmlComments;

            var xmlComments = this.GetXmlComments(tMethod);

            // If the summary is empty, we need to obtain the value from one of the Super classes:
            var superType = tMethod.OuterClass.RealType.BaseType;
            while ((superType != null) && xmlComments.Summary.IsEmpty())
            {
                try
                {
                    // NB: If we can't find the super class, there's no point in continuing:
                    var tSuperClass = _AssemblyReader.GetTemplate(superType) as ClassTemplate;
                    if (tSuperClass == null)
                        return _EmptyXmlComments;

                    var tSuperMethod = tSuperClass.Methods.TryGetValueOrNull(tMethod.Name);
                    if (tSuperMethod == null)
                        return _EmptyXmlComments;

                    xmlComments = tSuperClass.XmlComments;
                }
                finally
                {
                    superType = superType.BaseType;
                }
            }

            return xmlComments;
        }


        public XmlComments GetXmlCommentsFor(ParameterTemplate tParam)
        {
            if (this.IsHelpAvailable == false)
                return _EmptyXmlComments;

            if (tParam.IsVisible == false)
                return _EmptyXmlComments;

            var xmlComments = this.GetXmlComments(tParam);

            var methodSignature = tParam.OuterMethod.Parameters.Values
                                    .Select(p => p.ParameterType).ToArray();

            // If the summary is empty, we need to obtain the value from one of the Super classes:
            var superType = tParam.OuterClass.RealType.BaseType;
            while ((superType != null) && xmlComments.Summary.IsEmpty())
            {
                try
                {
                    // NB: If we can't find the super class, there's no point in continuing:
                    var tSuperClass = _AssemblyReader.GetTemplate(superType) as ClassTemplate;
                    if (tSuperClass == null)
                        return _EmptyXmlComments;

                    var tSuperMethod = tSuperClass.Methods.TryGetValueOrNull(tParam.OuterMethod.Name);
                    if (tSuperMethod == null)
                        return _EmptyXmlComments;

                    var superMethodSignature = tSuperMethod.Parameters.Values
                                                .Select(p => p.ParameterType).ToArray();

                    // Look for a method that has the same name and signature:
                    var tSuperParam = tSuperMethod.Parameters.Values
                                        .SingleOrDefault(p => p.Name == tParam.Name &&
                                                             methodSignature.SequenceEqual(superMethodSignature));
                    if (tSuperParam == null)
                        return _EmptyXmlComments;

                    xmlComments = tSuperClass.XmlComments;
                }
                finally
                {
                    superType = superType.BaseType;
                }
            }

            return xmlComments;
        }

        /// <summary>
        /// Populates the given Class Template with text from the assembly XML file
        /// </summary>
        /// <param name="tClass"></param>

        private XmlComments GetXmlComments(ClassTemplate tClass)
        {
            var result = new XmlComments()
            {
                Summary = SearchForComment("T", "summary", string.Empty, string.Empty, tClass.RealType),
                Remarks = SearchForComment("T", "remarks", string.Empty, string.Empty, tClass.RealType),
            };
            return result;
        }

        /// <summary>
        /// Populates the given Method Template with text from the assembly XML file
        /// </summary>
        /// <param name="tMethod"></param>

        private XmlComments GetXmlComments(MethodTemplate tMethod)
        {
            var result = new XmlComments()
            {
                Summary = SearchForComment("M", "summary", tMethod.MethodInfo.Name, string.Empty, tMethod.MethodInfo.ReflectedType),
                Remarks = SearchForComment("M", "remarks", tMethod.MethodInfo.Name, string.Empty, tMethod.MethodInfo.ReflectedType),
                Value = SearchForComment("M", "value", tMethod.MethodInfo.Name, string.Empty, tMethod.MethodInfo.ReflectedType),
            };
            return result;
        }

        /// <summary>
        /// Populates the given Property Template with text from the assembly XML file
        /// </summary>
        /// <param name="tProperty"></param>

        private XmlComments GetXmlComments(PropertyTemplate tProperty)
        {
            var result = new XmlComments()
            {
                Summary = SearchForComment("P", "summary", tProperty.PropertyInfo.Name, string.Empty, tProperty.PropertyInfo.ReflectedType),
                Remarks = SearchForComment("P", "remarks", tProperty.PropertyInfo.Name, string.Empty, tProperty.PropertyInfo.ReflectedType),
                Value = SearchForComment("P", "value", tProperty.PropertyInfo.Name, string.Empty, tProperty.PropertyInfo.ReflectedType),
            };
            return result;
        }

        /// <summary>
        /// Populates the given Parameter Template with text from the assembly documentation XML file
        /// </summary>
        /// <param name="tParameter"></param>

        private XmlComments GetXmlComments(ParameterTemplate tParameter)
        {
            var result = new XmlComments()
            {
                Summary = SearchForComment("M", "value", tParameter.ParameterInfo.Member.Name, tParameter.Name, tParameter.ParameterInfo.Member.ReflectedType),
            };
            return result;
        }

        public XmlComments GetXmlCommentsFor(EnumTemplate tEnum)
        {
            var result = new XmlComments()
            {
                Summary = SearchForComment("T", "summary", tEnum.FullName, string.Empty, tEnum.RealType),
                Remarks = SearchForComment("T", "remarks", tEnum.FullName, string.Empty, tEnum.RealType),
            };
            return result;
        }

        public XmlComments GetXmlCommentsFor(EnumTemplate tEnum, EnumItemTemplate tEnumItem)
        {
            var declaringType = tEnum.RealType.DeclaringType;
            string fieldName = string.Empty;

            if (declaringType == null)
            {
                // This deals with enums:
                fieldName = tEnumItem.Name;
                declaringType = tEnum.RealType;
            }
            else
            {
                fieldName = string.Concat(tEnum.Name, ".", tEnumItem.Name);
            }

            var result = new XmlComments()
            {
                Summary = SearchForComment("F", "summary", fieldName, string.Empty, declaringType),
                Remarks = SearchForComment("F", "remarks", fieldName, string.Empty, declaringType),
            };
            return result;
        }

        /// <summary>
        /// Searches for the comment for the given arguments
        /// </summary>
        /// <param name="searchType">Class="TClass", Method="M", and Property="P"</param>
        /// <param name="commentType">"summary", "value", or "remarks"</param>
        /// <param name="memberName">The name of the member. May be omitted if the SearchType is "TClass".</param>
        /// <param name="parameterName">The name of the parameter (if searching for one). Only applicable if the SearchType is "M" or "P".</param>
        /// <param name="startingClass">The class to search</param>
        /// <returns>A String containing the comment. If no comment is found, an empty string is returned.</returns>

        private string SearchForComment(string searchType, string commentType, string memberName, string parameterName, Type startingClass)
        {
            var classType = startingClass;
            var comment = string.Empty;

            // Figure out what the XML Member Name should be (see the XML format to understand):
            var xmlMemberName = string.Empty;

            if (classType.IsEnum && memberName.IsNotEmpty())
            {
                xmlMemberName = string.Concat(searchType, ":", startingClass.FullName, ".", memberName);
            }
            else if (classType.IsEnum)
            {
                string className = classType.FullName.Replace("+", ".");
                xmlMemberName = string.Concat(searchType, ":", className);
            }
            else if (searchType == "T")
            {
                xmlMemberName = string.Concat(searchType, ":", classType.FullName);
            }
            else
            {
                xmlMemberName = string.Concat(searchType, ":", classType.FullName, ".", memberName);
            }

            // Get the value from the XML:
            if (searchType == "T")
            {
                comment = GetClassComment(xmlMemberName, commentType);
            }
            else if (parameterName.IsEmpty())
            {
                comment = GetMemberComment(xmlMemberName, commentType);
            }
            else
            {
                comment = GetParameterComment(xmlMemberName, parameterName);
            }

            if (comment.IsNotEmpty() && comment.Contains(Environment.NewLine))
            {
                // We need to remove any extra spaces after the carriage returns:
                var pattern = string.Concat(Environment.NewLine, "[ ]*");
                comment = System.Text.RegularExpressions.Regex.Replace(comment, pattern, Environment.NewLine);
            }

            return comment;
        }

        /// <summary>
        /// Returns the codebase of the Assembly as a file path (not a URI)
        /// </summary>
        /// <param name="assembly"></param>

        private string GetAssemblyLocation(Assembly assembly)
        {
            var path = assembly.CodeBase.Replace(@"file:///", string.Empty);
            return path.Replace(@"/", @"\");
        }

        private void LoadXmlCommentsFor(Assembly domainAssembly)
        {
            var filePath = this.GetAssemblyLocation(domainAssembly);
            var xmlHelpFile = Path.Combine(Path.GetDirectoryName(filePath), domainAssembly.GetName().Name + ".xml");
            if (!File.Exists(xmlHelpFile))
                return;

            var xpathDoc = new XPathDocument(xmlHelpFile);
            // The XPathNavigator is lighter and faster than an XmlDocument, so we'll use it instead:
            _XPathNavigator = xpathDoc.CreateNavigator();
        }

        /// <summary>
        /// Returns the comment text for the given Class and comment type
        /// </summary>
        /// <param name="xmlMemberName">The XPath that represents the Member. See the XML documentation for examples.</param>
        /// <param name="commentType">"summary", "value", or "remarks"</param>
        /// <returns>The comment value as a String. If no comment is found, an empty string is returned.</returns>

        private string GetClassComment(string xmlMemberName, string commentType)
        {
            if (_XPathNavigator == null)
                return string.Empty;

            var xPath = string.Concat(XPATH_ROOT, " [@name = '", xmlMemberName, "']/", commentType);
            var comment = _XPathNavigator.SelectSingleNode(xPath);
            if (comment == null)
                return string.Empty;

            return comment.Value.Trim();
        }

        /// <summary>
        /// Returns the comment text for the given Member and comment type
        /// </summary>
        /// <param name="xmlMemberName">The XPath that represents the Member. See the XML documentation for examples.</param>
        /// <param name="commentType">"summary", "value", or "remarks"</param>
        /// <returns>The comment value as a String. If no comment is found, an empty string is returned.</returns>

        private string GetMemberComment(string xmlMemberName, string commentType)
        {
            if (_XPathNavigator == null)
                return string.Empty;

            var xPath = string.Concat(XPATH_ROOT, " [starts-with(@name,'", xmlMemberName, "')]/", commentType);
            var comment = _XPathNavigator.SelectSingleNode(xPath);
            if (comment == null)
                return string.Empty;

            return comment.Value.Trim();
        }

        /// <summary>
        /// Returns the comment text for the given Member and Parameter
        /// </summary>
        /// <param name="xmlMemberName">The XPath that represents the Member. See the XML documentation for examples.</param>
        /// <param name="paramName">The name of the Parameter.</param>
        /// <returns>The comment value as a String. If no comment is found, an empty string is returned.</returns>
        /// <remarks>
        /// TODO: This doesn't handle overloaded methods very well.
        /// We would need to located the XML node using the FULL MEMBER SIGNATURE to guarantee a correct XPath match.
        /// </remarks>
        private string GetParameterComment(string xmlMemberName, string paramName)
        {
            if (_XPathNavigator == null)
                return string.Empty;

            // Because the member has parameters, the text must contain brackets.
            // This will help find the correct signature in the case of overloaded Methods:
            xmlMemberName = string.Concat(xmlMemberName, "(");
            string memberXPath = string.Concat(XPATH_ROOT, " [starts-with(@name,'", xmlMemberName, "')]");
            var member = _XPathNavigator.SelectSingleNode(memberXPath);
            if (member == null)
                return string.Empty;

            var paramXPath = string.Concat("param [(@name='", paramName, "')]");
            var param = member.SelectSingleNode(paramXPath);
            if (param == null)
                return string.Empty;

            return param.Value.Trim();
        }
    }
}