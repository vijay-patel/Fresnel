﻿using Envivo.Fresnel.Core;
using Envivo.Fresnel.UiCore.Messages;
using Envivo.Fresnel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envivo.Fresnel.UiCore
{
    public class SessionVmBuilder
    {
        private IClock _Clock;
        private Engine _Engine;

        public SessionVmBuilder
            (
            Engine engine,
            IClock clock
            )
        {
            _Engine = engine;
            _Clock = clock;
        }

        public SessionVM Build()
        {
            var result = new SessionVM()
            {
                UserName = Environment.UserName,
                LogonTime = _Clock.Now,
                InfoMessages = this.CreateInfoMessages(),
            };
            return result;
        }

        private IEnumerable<MessageVM> CreateInfoMessages()
        {
            var results = new List<MessageVM>();

            results.Add(new MessageVM() { OccurredAt = _Clock.Now, Text = "Welcome to Fresnel" });

            // TODO: Add system & memory info

            results.Add(new MessageVM() { OccurredAt = _Clock.Now, Text = Environment.UserName + " logged on" });

            return results;
        }
    }
}
