﻿using OlcSideScrollingConsoleGame.Models.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Commands
{
    public class Quest
    {
        public Quest()
        {

        }
        public string Name { get; set; }
        public bool Completed { get; set; } = false;
        public static ScriptProcessor Script { get; set; }
        public static Program Engine { get; set; }

        public virtual bool OnInteraction(List<DynamicGameObject> vecDynobs, DynamicGameObject target, Enum.NATURE nature) { return true; }

        public virtual bool PopulateDynamics(List<DynamicGameObject> vecDyns, string sMap) { return true; }

    }
}