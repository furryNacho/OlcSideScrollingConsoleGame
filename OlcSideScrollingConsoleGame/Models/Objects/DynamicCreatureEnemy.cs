using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models.Objects
{
    public class DynamicCreatureEnemy : Creature
    {

        public DynamicCreatureEnemy() : base("enemyzero", Core.Aggregate.Instance.GetSprite("enemyzero"))
        {
            Friendly = false;
            Health = 2;
            MaxHealth = 2;
            SolidVsDynamic = true;
            SolidVsMap = true;
            DamageGiven = 4;
        }

        public override void Behaviour(float fElapsedTime, DynamicGameObject player = null)
        {
            if (Health <= 0)
            {
                vx = 0;
                vy = 0;
                SolidVsDynamic = false;
                IsAttackable = false;
                return;
            }

            if (player != null)
            {
                // För att jaga player. 

                // no default behaviour
                // Check if player is nearby
                float fTargetX = player.px - px;
                float fTargetY = player.py - py;
                float fDistance = (float)Math.Sqrt(fTargetX * fTargetX + fTargetY * fTargetY);

                StateTick -= fElapsedTime;

                if (StateTick <= 0.0f) // för att inte göra beslut så ofta. 
                {
                    if (fDistance < 6.0f)
                    {
                        vx = (fTargetX / fDistance) * 2.0f;
                        vy = (fTargetY / fDistance) * 2.0f;

                        if (fDistance < 1.5f) // för att attakera med projektil
                            PerformAttack();
                    }
                    else
                    {
                        vx = 0;
                        vy = 0;
                    }

                    StateTick += 1.0f;
                }
            }

        }
    }
}
