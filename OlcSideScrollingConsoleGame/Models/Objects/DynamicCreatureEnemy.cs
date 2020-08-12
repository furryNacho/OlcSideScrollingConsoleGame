using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models.Objects
{

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
                        var vändåt = GetFacingDirection();
                        //FacingDirection = Enum.Direction.EAST;

                        if (vändåt == 1)
                        {
                            //vänster Enum.Direction.WEST
                            vx = 0.12f;
                        }
                        else if (vändåt == 3)
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
                    else if (Patrol == Enum.Actions.Right)
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

        public Enum.LastStage State { get; set; }

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

            //Temp, för test i demo endast. Byt ut när boss är bra först

            if (player != null)
            {
                Ticker++;

                StateTick -= fElapsedTime;

                if (StateTick <= 0.0f) // för att inte göra beslut så ofta. 
                {
                    StateTick += 1.0f;

                }


                


                if (Ticker < 60)
                {
                    State = Enum.LastStage.MovingDown;
                    
                }
                else if (Ticker < 120)
                {
                    State = Enum.LastStage.MovingUp;

                    

                }
                else if (Ticker >= 180)
                {
                    Ticker = 0;

                }




                if (py < 12 || py > 13)
                {
                    if (py < 12)
                    {
                        py = 12;
                    }
                    if (py > 13)
                    {
                        py = 13;
                    }
                }

                if (State == Enum.LastStage.MovingDown)
                {
                    if (vy <= 0)
                    {
                    }
                    vy = 2;

                    //if (py != 13)
                    //{

                    //    py = 13;
                    //}

                }
                else if (State == Enum.LastStage.MovingUp)
                {
                    if (vy >= 0)
                    {
                    }
                    vy = -2;
                }



                vx = 0;
                if (py <= FromCor)
                {
                    vy = 0;
                    py = 12;
                }
                if (py >= ToCor)
                {
                    vy = 0;
                    py = 13;
                }

            }
            //end temp

        }
    }




    public class DynamicCreatureEnemyBoss : Creature
    {

        public DynamicCreatureEnemyBoss() : base("enemyboss", Core.Aggregate.Instance.GetSprite("enemyboss"))
        //public DynamicCreatureEnemyBoss() : base("enemyboss", Core.Aggregate.Instance.GetSprite("enemyzero"))
        {
            Friendly = false;
            Health = 30;
            MaxHealth = 50;
            SolidVsDynamic = true;
            SolidVsMap = true;
            DamageGiven = 10;
        }

        public Enum.LastStage State { get; set; }


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
                Ticker++;

                StateTick -= fElapsedTime;





                if (StateTick <= 0.0f) // för att inte göra beslut så ofta. 
                {
                    StateTick += 1.0f;

                }



                //if (!IsAttackable)
                //{
                //    Ticker = 0;
                //}

                //
                //if (State == Enum.LastStage.StayUp && Ticker < 120)
                //{
                //    State = Enum.LastStage.MovingDown;
                //}
                //else if (State == Enum.LastStage.MovingDown && Ticker > 120)
                //{
                //    State = Enum.LastStage.StayDown;
                //}
                //else if (State == Enum.LastStage.StayDown && Ticker > 150)
                //{
                //    State = Enum.LastStage.MovingUp;
                //}
                //else if (State == Enum.LastStage.MovingUp && Ticker > 270)
                //{
                //    State = Enum.LastStage.StayUp;
                //}
                //else if (Ticker >= 300)
                //{
                //    Ticker = 0;
                //}
                if (Ticker < 60)
                {
                    State = Enum.LastStage.MovingDown;
                }
                else if (Ticker < 120)
                {
                    State = Enum.LastStage.MovingUp;

                    if (Ticker < 118)
                    {
                        TurnedTo = Enum.PlayerOrientation.Left;
                    }

                }else if (Ticker < 160)
                {
                    TurnedTo = Enum.PlayerOrientation.Right;
                }
                else if (Ticker >= 180)
                {
                    Ticker = 0;
                }



               
                //



                if (py < 12 || py > 13)
                {
                    if (py < 12)
                    {
                        py = 12;
                    }
                    if (py > 13)
                    {
                        py = 13;
                    }
                }

                if (State == Enum.LastStage.MovingDown)
                {
                    if (vy <= 0)
                    {
                    }
                    vy = 2;

                    //if (py != 13)
                    //{
                    //    //py = py + 0.20f;
                    //    py = 13;
                    //}

                }
                else if (State == Enum.LastStage.MovingUp)
                {
                    if (vy >= 0)
                    {
                    }
                    vy = -2;
                }



                vx = 0;
                if (py <= FromCor)
                {
                    vy = 0;
                    py = 12;
                }
                if (py >= ToCor)
                {
                    vy = 0;
                    py = 13;
                }


                //reset IsAttackable
                if (!IsAttackable && py == 13)
                {
                    IsAttackable = true;
                }


                // Switch X
                if (py == 13.0f)
                {
                    Core.Aggregate.Instance.IsUnderGround = true;
                    Core.Aggregate.Instance.IsMoving = false;
                }
                else if (py == 12.0f)
                {
                    Core.Aggregate.Instance.IsAboveGround = true;
                    Core.Aggregate.Instance.IsMoving = false;
                }
                else
                {
                    Core.Aggregate.Instance.IsMoving = true;
                    Core.Aggregate.Instance.IsUnderGround = false;
                    Core.Aggregate.Instance.IsAboveGround = false;
                }
               



                //if (State == Enum.LastStage.StayUp)
                //{
                //    py = 12.0f;
                //}
                //else if (State == Enum.LastStage.MovingUp)
                //{
                //    if (py > 12.0f)
                //    {
                //        py = py - 0.025f;
                //    }
                //    else
                //    {
                //        py = 12.0f;
                //    }
                //}
                //else if (State == Enum.LastStage.StayDown)
                //{
                //    py = 13.0f;
                //}
                //else if (State == Enum.LastStage.MovingDown)
                //{
                //    if (py < 13.0f)
                //    {
                //        py = py + 0.025f;
                //    }
                //    else
                //    {
                //        py = 13.0f;
                //    }
                //}






            }

        }

        #region DrawSelf



        //public override void DrawSelf(Program gfx, float ox, float oy)  // gfx  = graphics //  olcConscoleGameEngineOOP
        //{
        //    int spriteSize = 32;
        //    // Måste draw rätt sprite som passar state som creature är i, in this point in time

        //    // Mosvarar vart på spriten som ska ritas. 

        //    int SheetOffsetX = 0; //Uppe till vänster är sheet offset 0. (noll index)
        //    int SheetOffsetY = 0;// Om y är 1 så är det en rad ner (noll index)

        //    switch (sprGraphicsState)
        //    {
        //        /*
        //         * längst ner i högra hörnet (5x5)
        //           SheetOffsetY = 4 * 16; (Vertikalen)
        //           SheetOffsetX = 4 * 16; (Horizontalen)
        //         */
        //        /*
        //         Right = 0,
        //         Left = 1
        //         */

        //        case Enum.GraphicsState.Standing:

        //            if (IsIdle)
        //            {

        //                //var asdf = this is DynamicCreatureHero;
        //                //if (asdf) // IsHero
        //                //{
        //                //    var qwer = true;
        //                //}

        //                int oscillera = (int)TurnedTo == 0 ? 0 : 2;
        //                if (GraphicCounter % 2 == 0)
        //                {
        //                    SheetOffsetY = 4 * spriteSize;
        //                    SheetOffsetX = (0 + oscillera) * spriteSize;
        //                }
        //                else
        //                {
        //                    SheetOffsetY = 4 * spriteSize;
        //                    SheetOffsetX = (1 + oscillera) * spriteSize;
        //                }

        //                break;
        //            }

        //            //if (IsHero) // IsHero
        //            //{
        //            //    var asdf = this as DynamicCreatureHero;
        //            //    if (asdf.LookDown)
        //            //    {
        //            //        // rad tre för höger
        //            //        // rad fyra för vänster
        //            //        SheetOffsetY = (int)(TurnedTo + 2) * 16;
        //            //        // kolumn fyra för titta ner
        //            //        SheetOffsetX = 48;
        //            //        break;
        //            //    }

        //            //    if (asdf.LookUp)
        //            //    {
        //            //        // rad tre för höger
        //            //        // rad fyra för vänster
        //            //        SheetOffsetY = (int)(TurnedTo + 2) * 16;
        //            //        // tredje komlumn för titta upp
        //            //        SheetOffsetX = 32;
        //            //        break;
        //            //    }
        //            //}


        //            SheetOffsetX = 0;
        //            SheetOffsetY = (int)TurnedTo * 16;
        //            break;

        //        case Enum.GraphicsState.Walking:
        //            //SheetOffsetX = (int)FacingDirection * 16; // så typ den övre raden är åt vilket håll, sen switcha mellan övre raden och undre raden i hans sprite.
        //            //SheetOffsetY = (int)FacingDirection * 16;
        //            SheetOffsetX = GraphicCounter * 16;
        //            SheetOffsetY = (int)TurnedTo * 16;
        //            break;

        //        case Enum.GraphicsState.Jumping:

        //            SheetOffsetX = 0 * 16;
        //            SheetOffsetY = ((int)TurnedTo + 2) * 16;

        //            break;

        //        case Enum.GraphicsState.Falling:

        //            SheetOffsetX = 1 * 16;
        //            SheetOffsetY = ((int)TurnedTo + 2) * 16;

        //            break;

        //        case Enum.GraphicsState.Celebrating:
        //            SheetOffsetX = 4 * 16;
        //            SheetOffsetY = 4 * 16;
        //            break;

        //        case Enum.GraphicsState.Dead:

        //            SheetOffsetX = GraphicCounter * 16;
        //            SheetOffsetY = 4 * 16;
        //            break;

        //        case Enum.GraphicsState.TakingDamage:

        //            SheetOffsetY = (int)(TurnedTo + 2) * 16;
        //            SheetOffsetX = 16 * 4;

        //            break;
        //    }


        //    //Sen är det dags att rita ut spriten
        //    // dynamiska objektet finns i world space, men måste rita den i screen space. 1 - 1 translation eftersom alla enheter är en / en enheter.
        //    //Vi måste bara ta reda på vart kameran titar i world space.
        //    var firstMagicalPlayerParam = new Point();
        //    if (IsHero)
        //    {
        //        var notGoOfTheWorldLeft = 0.0f;
        //        notGoOfTheWorldLeft = (px - ox) * 16.0f;
        //        if (notGoOfTheWorldLeft <= 1.0f)
        //        {
        //            notGoOfTheWorldLeft = 1.0f;
        //        }
        //        firstMagicalPlayerParam = new Point((int)notGoOfTheWorldLeft, (int)((py - oy) * 16.0f));
        //    }
        //    else
        //    {
        //        //  firstMagicalPlayerParam = new Point((int)((px - ox) * 16.0f), (int)((py - oy) * 16.0f)); // Vart tilen ska ritas.
        //        firstMagicalPlayerParam = new Point((int)((px - ox) * 16.0f), (int)((py - oy) * 16.0f));
        //    }

        //    // SheetOffsetX och SheetOffsetY ger top left in en sprite
        //    var secondMagicalPlayerParam = new Point(SheetOffsetX, SheetOffsetY); // Vilken tile i spritesheeten som ska ritas.

        //    // 16 är för närvarande en full enhet 
        //    gfx.DrawPartialSprite(firstMagicalPlayerParam, Sprite, secondMagicalPlayerParam, spriteSize, spriteSize);

        //}
        #endregion
    }

    public class DynamicCreatureOverlay : Creature
    {

        public DynamicCreatureOverlay() : base("overlay", Core.Aggregate.Instance.GetSprite("enemyboss"))
        {
            Friendly = true;
            Health = 100;
            MaxHealth = 100;
            SolidVsDynamic = false;
            SolidVsMap = true;
            DamageGiven = 0;
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

}
