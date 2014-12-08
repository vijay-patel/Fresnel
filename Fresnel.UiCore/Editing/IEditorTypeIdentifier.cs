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
    public interface IEditorTypeIdentifier
    {
        bool CanHandle(BasePropertyObserver oProp);

        EditorType DetermineEditorType(BasePropertyObserver oProp);
    }
}