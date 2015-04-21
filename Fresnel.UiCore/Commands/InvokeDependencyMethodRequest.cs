﻿using Envivo.Fresnel.UiCore.Model;
using System;
using System.Collections.Generic;
using T4TS;

namespace Envivo.Fresnel.UiCore.Commands
{
    [TypeScriptInterface]
    public class InvokeDependencyMethodRequest
    {
        public string ClassType { get; set; }

        public string MethodName { get; set; }

        public ParameterVM[] Parameters { get; set; }

    }
}