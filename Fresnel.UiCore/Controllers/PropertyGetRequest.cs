﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Envivo.Fresnel.UiCore.Controllers
{
    public class PropertyGetRequest
    {
        public Guid ObjectID { get; set; }

        public string PropertyName { get; set; }
    }
}
