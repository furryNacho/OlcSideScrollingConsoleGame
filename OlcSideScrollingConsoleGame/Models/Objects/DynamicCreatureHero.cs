#nullable enable
using OlcSideScrollingConsoleGame.Core;
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
        public DynamicCreatureHero(IAssets assets) : base("hero", assets.GetSprite("hero"))
        {
            Friendly = true;
            Health = 9;
            MaxHealth = 100;
            StateTick = 2.0f;
            IsHero = true;
        }

        /// <summary>
        /// Hjältens beteende styrs av Program.cs via IInputProvider — inget autonomt AI-beteende.
        /// </summary>
        public override void Behaviour(float fElapsedTime, DynamicGameObject? player = null) { }
    }
}
