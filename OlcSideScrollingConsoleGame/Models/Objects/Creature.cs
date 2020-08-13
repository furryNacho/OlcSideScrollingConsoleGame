using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models.Objects
{
    public class Creature : DynamicGameObject
    {
        public Creature(string name, Sprite sprite)
            : base(name)

        {
            this.Sprite = sprite;
            Health = 5;
            MaxHealth = 10;
            FacingDirection = Enum.Direction.EAST;
            sprGraphicsState = Enum.GraphicsState.Falling;
            Timer = 0.0f;
            GraphicCounter = 0;
            IsAttackable = true;
        }

        //States
        private Enum.Direction FacingDirection { get; set; }
        protected Enum.GraphicsState sprGraphicsState { get; set; }
        protected int GraphicCounter { get; set; }
        private float Timer { get; set; }
        protected Sprite Sprite { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int DamageGiven { get; set; } = 0;

        public int Ticker { get; set; } = 0;
        public bool DoSpecialAction { get; set; } = false;
        public bool HasJumped { get; set; } = false;
        public int DoneSpecialAction { get; set; } = 0;

       

        public override void Update(float elapsedTime, DynamicGameObject player)
        {
            if (KnockBackTimer > 0.0f)
            {
                // Flyttas till vänster eller höger, sätt hjälten till motsatt riktning

                if (vx < 0)
                {
                    TurnedTo = Enum.PlayerOrientation.Right;
                }
                else
                {
                    TurnedTo = Enum.PlayerOrientation.Left;

                }

                vx = KnockBackDX * 3.0f;
                vy = KnockBackDY * 2.0f;
                IsAttackable = false;
                KnockBackTimer -= elapsedTime;
                sprGraphicsState = Enum.GraphicsState.TakingDamage;

                if (KnockBackTimer <= 0.0f)
                {
                    StateTick = 0.0f;
                    Controllable = true;
                    IsAttackable = true;
                }
            }
            else
            {
                //if (IsHero)
                //{


                // En bubbla av tid som förflyttar sig oberoende av resten av spelets state. Skapar ett slags pendel, gissar jag på.
                Timer += elapsedTime;
                if (Timer <= 0.1f)
                {
                    //GraphicCounter = 0;
                    GraphicCounter = 1;

                }
                else if (Timer <= 0.2f)
                {
                    //GraphicCounter = 1;
                    GraphicCounter = 2;

                }
                else if (Timer <= 0.3f)
                {
                    //GraphicCounter = 2;
                    GraphicCounter = 3;

                }
                //else if (Timer <= 0.4f)
                //{
                //    //GraphicCounter = 3;
                //    GraphicCounter = 4;

                //}
                //else if (Timer <= 0.5f)
                //{
                //    GraphicCounter = 4;
                //    Timer = 0.0f;
                //}
                else
                {
                    GraphicCounter = 4;
                    Timer = 0.0f;

                }
                //Timer += elapsedTime;
                //if (Timer >= 0.2f)
                //{
                //    Timer -= 0.2f;

                    //    // tänkt att "ocilera" (vad fan det nu är på svenska). ticka mellan 0 och 1
                    //    GraphicCounter++;
                    //    GraphicCounter %= 2;
                    //}


                    //   }


                if (Math.Abs(vy) > 0 && !IsIdle)
                {
                    if (vy < 0)
                    {
                        sprGraphicsState = Enum.GraphicsState.Jumping;
                    }
                    else
                    {
                        sprGraphicsState = Enum.GraphicsState.Falling;
                    }

                    
                }
                else if (Math.Abs(vx) > 1.0f )
                {
                    sprGraphicsState = Enum.GraphicsState.Walking;
                }
                else
                {
                    sprGraphicsState = Enum.GraphicsState.Standing;
                }

                if (Health <= 0)
                {
                    sprGraphicsState = Enum.GraphicsState.Dead;
                }


                if (vx < -0.1f)
                {
                    FacingDirection = Enum.Direction.WEST;
                    TurnedTo = Enum.PlayerOrientation.Left;
                }
                if (vx > 0.1f)
                {
                    FacingDirection = Enum.Direction.EAST;
                    TurnedTo = Enum.PlayerOrientation.Right;
                }
                if (vy < -0.1f)
                {
                    FacingDirection = Enum.Direction.NORTH;
                }
                if (vy > 0.1f)
                {
                    FacingDirection = Enum.Direction.SOUTH;
                }

                Behaviour(elapsedTime, player);

            }
        }


        public override void DrawSelf(Program gfx, float ox, float oy)  // gfx  = graphics //  olcConscoleGameEngineOOP
        {
            // Måste draw rätt sprite som passar state som creature är i, in this point in time

            // Mosvarar vart på spriten som ska ritas. 
            
            int SheetOffsetX = 0; //Uppe till vänster är sheet offset 0. (noll index)
            int SheetOffsetY = 0;// Om y är 1 så är det en rad ner (noll index)

            if (Name == "ice")
            {
                SheetOffsetY = 1 * 16;
                SheetOffsetX = 4 * 16;
            }
            else if (Name == "boss")
            {

                if (!IsAttackable)
                {
                    SheetOffsetY = 1 * 16;
                    SheetOffsetX = 0 * 16; 
                }
                else
                {
                    if (this is DynamicCreatureEnemyBoss)
                    {
                        var bossman = this as DynamicCreatureEnemyBoss;
                        if (bossman.State == Enum.LastStage.MovingUp)
                        {

                            if (py <= 12)
                            {
                                //"idle", vrid på huvudet
                                if (TurnedTo == Enum.PlayerOrientation.Left)
                                {
                                    SheetOffsetY = 0 * 16;
                                    SheetOffsetX = 4 * 16;
                                }
                                else if (TurnedTo == Enum.PlayerOrientation.Right)
                                {
                                    SheetOffsetY = 0 * 16;
                                    SheetOffsetX = 3 * 16;
                                }


                            }
                            else
                            {
                                //flaxa lite på vägen upp
                                GraphicCounter = GraphicCounter - 1;

                                SheetOffsetY = 0 * 16;
                                SheetOffsetX = GraphicCounter * 16;
                            }

                            
                        }
                    }
                    else
                    {
                        //fall ner
                        SheetOffsetY = 1 * 16;
                        SheetOffsetX = 4 * 16;

                    }
                }
            }
            else if (Name == "overlay")
            {
                SheetOffsetY = 3 * 16;
                SheetOffsetX = 4 * 16;
            }
            //else if (Name == "overlayworldmap")
            //{
            //    SheetOffsetY = 0 * 16;
            //    SheetOffsetX = 0 * 16;
            //}
            else
            {
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
                                SheetOffsetY = 4 * 16;
                                SheetOffsetX = (0 + oscillera) * 16;
                            }
                            else
                            {
                                SheetOffsetY = 4 * 16;
                                SheetOffsetX = (1 + oscillera) * 16;
                            }

                            break;
                        }

                        if (IsHero) // IsHero
                        {
                            var asdf = this as DynamicCreatureHero;
                            if (asdf.LookDown)
                            {
                                // rad tre för höger
                                // rad fyra för vänster
                                SheetOffsetY = (int)(TurnedTo + 2) * 16;
                                // kolumn fyra för titta ner
                                SheetOffsetX = 48;
                                break;
                            }

                            if (asdf.LookUp)
                            {
                                // rad tre för höger
                                // rad fyra för vänster
                                SheetOffsetY = (int)(TurnedTo + 2) * 16;
                                // tredje komlumn för titta upp
                                SheetOffsetX = 32;
                                break;
                            }
                        }


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
                firstMagicalPlayerParam = new Point((int)((px - ox) * 16.0f), (int)((py - oy) * 16.0f)); // Vart tilen ska ritas.
            }

            // SheetOffsetX och SheetOffsetY ger top left in en sprite
            var secondMagicalPlayerParam = new Point(SheetOffsetX, SheetOffsetY); // Vilken tile i spritesheeten som ska ritas.

            // 16 är för närvarande en full enhet 
            gfx.DrawPartialSprite(firstMagicalPlayerParam, Sprite, secondMagicalPlayerParam, 16, 16);

        }

        public virtual void Behaviour(float fElapsedTime, DynamicGameObject player = null) { }

        public virtual void PerformAttack() { }

        public virtual void KnockBack(float dx, float dy, float dist)
        {
            KnockBackDX = dx;
            KnockBackDY = dy;
            KnockBackTimer = dist;
            SolidVsDynamic = false;
            Controllable = false;
            IsAttackable = false;
        }

        public virtual void ChangeStageKnockBackReset()
        {
            KnockBackDX = 0;
            KnockBackDY = 0;
            KnockBackTimer = 0;
            SolidVsDynamic = true;
            Controllable = true;
            IsAttackable = true;
        }

        public int GetFacingDirection()
        {
            return (int)FacingDirection;
        }

        public float StateTick { get; set; }

    }

}
