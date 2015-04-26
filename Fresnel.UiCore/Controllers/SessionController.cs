﻿using Envivo.Fresnel.UiCore.Commands;
using Envivo.Fresnel.UiCore.Model;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;

namespace Envivo.Fresnel.UiCore.Controllers
{
    public class SessionController : ApiController
    {
        private SessionVmBuilder _SessionVmBuilder;
        private CleanupSessionCommand _CleanupSessionCommand;

        public SessionController
            (
            SessionVmBuilder sessionVmBuilder,
            CleanupSessionCommand cleanupSessionCommand
            )
        {
            _SessionVmBuilder = sessionVmBuilder;
            _CleanupSessionCommand = cleanupSessionCommand;
        }

        [HttpGet]
        public SessionVM GetSession()
        {
            return _SessionVmBuilder.Build();
        }

        [HttpGet]
        public GenericResponse CleanUp()
        {
            var result = _CleanupSessionCommand.Invoke();
            return result;
        }
    }
}