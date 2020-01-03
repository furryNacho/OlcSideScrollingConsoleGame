using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models.Objects
{
    public class DynamicCreatureHero : Creature
    {
        public bool LookUp { get; set; }
        public bool LookDown { get; set; }
        public DynamicCreatureHero() : base("hero", Core.Aggregate.Instance.GetSprite("hero"))
        {
            Friendly = true;
            Health = 9;
            MaxHealth = 100;
            StateTick = 2.0f;
            IsHero = true;
        }
    }
}
