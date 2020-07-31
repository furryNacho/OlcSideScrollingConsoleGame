using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models.Objects
{
    public class DynamicCreatureEnemyIcicle : Creature
    {

        public DynamicCreatureEnemyIcicle() : base("enemyzero", Core.Aggregate.Instance.GetSprite("enemyzero"))
        {
            Friendly = false;
            Health = 20;
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

            }

        }
    }

    public class DynamicCreatureEnemyPenguin : Creature
    {

        public DynamicCreatureEnemyPenguin() : base("enemyone", Core.Aggregate.Instance.GetSprite("enemyone"))
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
                        Ticker = 0;

                        vx = (fTargetX / fDistance) * 2.0f;
                        // vy = (fTargetY / fDistance) * 2.0f;

                        //if (fDistance < 1.5f) // för att attakera med projektil
                        //    PerformAttack();
                    }
                    else
                    {
                        vx = 0;
                        vy = 0;
                        Ticker++;
                    }

                    StateTick += 1.0f;


                    // enkel hantera om idle
                    if (Ticker > 7)
                    {
                        var vändåt= GetFacingDirection();
                        //FacingDirection = Enum.Direction.EAST;

                        if (vändåt == 1)
                        {
                            //vänster Enum.Direction.WEST
                            vx = 0.12f;
                        }else if (vändåt == 3)
                        {
                            // höger Enum.Direction.EAST
                            vx = -0.12f;
                        }

                        Ticker = 0;
                    }

                }
            }

        }
    }


    public class DynamicCreatureEnemyWalrus : Creature
    {

        public DynamicCreatureEnemyWalrus() : base("enemytwo", Core.Aggregate.Instance.GetSprite("enemytwo"))
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
                Patrol = Enum.Actions.Left;
                return;
            }

            if (player != null)
            {
                // patrol (and fear of heights)

                StateTick -= fElapsedTime;

                if (StateTick <= 0.0f) // för att inte göra beslut så ofta. 
                {
                    if (Patrol == Enum.Actions.Left)
                    {
                        vx = -2;
                    }
                    else if(Patrol == Enum.Actions.Right)
                    {
                        vx = 2;
                    }
                    else
                    {
                        vy = 0;
                        vx = 0;

                    }

                    StateTick += 1.0f;
                }
            }

        }
    }

    public class DynamicCreatureEnemyFrost : Creature
    {

        public DynamicCreatureEnemyFrost() : base("enemythree", Core.Aggregate.Instance.GetSprite("enemythree"))
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
                Patrol = Enum.Actions.Left;
                return;
            }

            if (player != null)
            {
                // patrol (and fear of heights)

                StateTick -= fElapsedTime;

                if (StateTick <= 0.0f) // för att inte göra beslut så ofta. 
                {
                    if (Patrol == Enum.Actions.Left)
                    {
                        vx = -1;
                    }
                    else if (Patrol == Enum.Actions.Right)
                    {
                        vx = 1;
                    }
                    else
                    {
                        vy = 0;
                        vx = 0;

                    }


                    StateTick += 1.0f;
                }
            }

        }
    }

    public class DynamicCreatureEnemyWind : Creature
    {

        public DynamicCreatureEnemyWind() : base("enemywind", Core.Aggregate.Instance.GetSprite("enemywind"))
        {
            Friendly = true;
            Health = 50;
            MaxHealth = 50;
            SolidVsDynamic = true;
            SolidVsMap = true;
            DamageGiven = 0;
        }

        public override void Behaviour(float fElapsedTime, DynamicGameObject player = null)
        {
            if (Health <= 0)
            {
                vx = 0;
                vy = 0;
                SolidVsDynamic = false;
                IsAttackable = false;
                // Patrol = Enum.Actions.Left;
                return;
            }

            if (player != null)
            {
                // patrol (and fear of heights)

                StateTick -= fElapsedTime;

                if (StateTick <= 0.0f) // för att inte göra beslut så ofta. 
                {
                    if (Patrol == Enum.Actions.Left)
                    {
                        vx = -2;
                    }
                    else if (Patrol == Enum.Actions.Right)
                    {
                        vx = 2;
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

    public class DynamicCreatureEnemyFrostBoss : Creature
    {

        public DynamicCreatureEnemyFrostBoss() : base("enemyboss", Core.Aggregate.Instance.GetSprite("enemyboss"))
        {
            Friendly = false;
            Health = 50;
            MaxHealth = 50;
            SolidVsDynamic = true;
            SolidVsMap = true;
            DamageGiven = 10;
        }

        public override void Behaviour(float fElapsedTime, DynamicGameObject player = null)
        {
            if (Health <= 0)
            {
                vx = 0;
                vy = 0;
                SolidVsDynamic = false;
                IsAttackable = false;
               // Patrol = Enum.Actions.Left;
                return;
            }

            if (player != null)
            {
                // patrol (and fear of heights)

                StateTick -= fElapsedTime;

                if (StateTick <= 0.0f) // för att inte göra beslut så ofta. 
                {
                    if (Patrol == Enum.Actions.Left)
                    {
                        vx = -2;
                    }
                    else if (Patrol == Enum.Actions.Right)
                    {
                        vx = 2;
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



        public override void DrawSelf(Program gfx, float ox, float oy)  // gfx  = graphics //  olcConscoleGameEngineOOP
        {
            int spriteSize = 32;
            // Måste draw rätt sprite som passar state som creature är i, in this point in time

            // Mosvarar vart på spriten som ska ritas. 

            int SheetOffsetX = 0; //Uppe till vänster är sheet offset 0. (noll index)
            int SheetOffsetY = 0;// Om y är 1 så är det en rad ner (noll index)

            switch (sprGraphicsState)
            {
                /*
                 * längst ner i högra hörnet (5x5)
                   SheetOffsetY = 4 * 16; (Vertikalen)
                   SheetOffsetX = 4 * 16; (Horizontalen)
                 */
                /*
                 Right = 0,
                 Left = 1
                 */

                case Enum.GraphicsState.Standing:

                    if (IsIdle)
                    {

                        //var asdf = this is DynamicCreatureHero;
                        //if (asdf) // IsHero
                        //{
                        //    var qwer = true;
                        //}

                        int oscillera = (int)TurnedTo == 0 ? 0 : 2;
                        if (GraphicCounter % 2 == 0)
                        {
                            SheetOffsetY = 4 * spriteSize;
                            SheetOffsetX = (0 + oscillera) * spriteSize;
                        }
                        else
                        {
                            SheetOffsetY = 4 * spriteSize;
                            SheetOffsetX = (1 + oscillera) * spriteSize;
                        }

                        break;
                    }

                    //if (IsHero) // IsHero
                    //{
                    //    var asdf = this as DynamicCreatureHero;
                    //    if (asdf.LookDown)
                    //    {
                    //        // rad tre för höger
                    //        // rad fyra för vänster
                    //        SheetOffsetY = (int)(TurnedTo + 2) * 16;
                    //        // kolumn fyra för titta ner
                    //        SheetOffsetX = 48;
                    //        break;
                    //    }

                    //    if (asdf.LookUp)
                    //    {
                    //        // rad tre för höger
                    //        // rad fyra för vänster
                    //        SheetOffsetY = (int)(TurnedTo + 2) * 16;
                    //        // tredje komlumn för titta upp
                    //        SheetOffsetX = 32;
                    //        break;
                    //    }
                    //}


                    SheetOffsetX = 0;
                    SheetOffsetY = (int)TurnedTo * 16;
                    break;

                case Enum.GraphicsState.Walking:
                    //SheetOffsetX = (int)FacingDirection * 16; // så typ den övre raden är åt vilket håll, sen switcha mellan övre raden och undre raden i hans sprite.
                    //SheetOffsetY = (int)FacingDirection * 16;
                    SheetOffsetX = GraphicCounter * 16;
                    SheetOffsetY = (int)TurnedTo * 16;
                    break;

                case Enum.GraphicsState.Jumping:

                    SheetOffsetX = 0 * 16;
                    SheetOffsetY = ((int)TurnedTo + 2) * 16;

                    break;

                case Enum.GraphicsState.Falling:

                    SheetOffsetX = 1 * 16;
                    SheetOffsetY = ((int)TurnedTo + 2) * 16;

                    break;

                case Enum.GraphicsState.Celebrating:
                    SheetOffsetX = 4 * 16;
                    SheetOffsetY = 4 * 16;
                    break;

                case Enum.GraphicsState.Dead:

                    SheetOffsetX = GraphicCounter * 16;
                    SheetOffsetY = 4 * 16;
                    break;

                case Enum.GraphicsState.TakingDamage:

                    SheetOffsetY = (int)(TurnedTo + 2) * 16;
                    SheetOffsetX = 16 * 4;

                    break;
            }


            //Sen är det dags att rita ut spriten
            // dynamiska objektet finns i world space, men måste rita den i screen space. 1 - 1 translation eftersom alla enheter är en / en enheter.
            //Vi måste bara ta reda på vart kameran titar i world space.
            var firstMagicalPlayerParam = new Point();
            if (IsHero)
            {
                var notGoOfTheWorldLeft = 0.0f;
                notGoOfTheWorldLeft = (px - ox) * 16.0f;
                if (notGoOfTheWorldLeft <= 1.0f)
                {
                    notGoOfTheWorldLeft = 1.0f;
                }
                firstMagicalPlayerParam = new Point((int)notGoOfTheWorldLeft, (int)((py - oy) * 16.0f));
            }
            else
            {
                //  firstMagicalPlayerParam = new Point((int)((px - ox) * 16.0f), (int)((py - oy) * 16.0f)); // Vart tilen ska ritas.
                firstMagicalPlayerParam = new Point((int)((px - ox) * 16.0f), (int)((py - oy) * 16.0f));
            }

            // SheetOffsetX och SheetOffsetY ger top left in en sprite
            var secondMagicalPlayerParam = new Point(SheetOffsetX, SheetOffsetY); // Vilken tile i spritesheeten som ska ritas.

            // 16 är för närvarande en full enhet 
            gfx.DrawPartialSprite(firstMagicalPlayerParam, Sprite, secondMagicalPlayerParam, spriteSize, spriteSize);

        }

    }

}
