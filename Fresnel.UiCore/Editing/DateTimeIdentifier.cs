﻿using Envivo.Fresnel.Configuration;
using Envivo.Fresnel.Core.Commands;
using Envivo.Fresnel.Core.Observers;
using Envivo.Fresnel.Introspection.Templates;
using Envivo.Fresnel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envivo.Fresnel.UiCore.Editing
{
    public class DateTimeIdentifier : IEditorTypeIdentifier
    {
        public bool CanHandle(BasePropertyObserver oProp)
        {
            var tClass = oProp.Template.InnerClass; 
            return tClass.RealType == typeof(DateTime) ||
                   tClass.RealType == typeof(DateTimeOffset);
        }

        public EditorType DetermineEditorType(BasePropertyObserver oProp)
        {
            return EditorType.DateTime;
        }
    }
}