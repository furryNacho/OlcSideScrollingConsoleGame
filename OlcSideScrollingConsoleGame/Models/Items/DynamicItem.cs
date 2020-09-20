using OlcSideScrollingConsoleGame.Models.Objects;
using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models.Items
{
    public class DynamicItem : DynamicGameObject
    {


        public DynamicItem(float x, float y, Item item, int Collectable = 0, int id = 0)
        : base("pickup")
        {
            px = x;
            py = y;
            SolidVsDynamic = false;
            SolidVsMap = false;
            Friendly = true;
            Collected = false;
            this.item = item;
            this.Collectable = Collectable;
            
            if (Collectable > 0)
            {
                IsTempEnergi = true;
            }
            CoinId = id;
        }



        public Item item { get; set; }
        public bool Collected { get; set; }
        /// <summary>
        /// 0 is Collectable
        /// </summary>
        public int Collectable { get; set; }
      

        public override void DrawSelf(Program graphics, float ox, float oy)
        {
            if (Collected)
            {
                //this.Redundant = true;
                this.RemoveCount = 5;
                return;
            }

            int spriteX = 0;
            int spriteY = 0;
            // TODO: animera fram energi - och bort
            if (this.IsTempEnergi)
            {
                // Animera in
                if (Collectable > 12)
                    spriteX = 16 * 4;
                else if (Collectable > 8)
                    spriteX = 16 * 3;
                else if (Collectable > 4)
                    spriteX = 16 * 2;
                else if (Collectable > 0)
                    spriteX = 16 * 1;
                // Animera ut
                else if (Collectable > -48)
                    spriteX = 16 * 1;
                else if (Collectable > -52)
                    spriteX = 16 * 2;
                else if (Collectable > -56)
                    spriteX = 16 * 3;
                else if (Collectable > -60) //Ett litet glapp innan ta bort för osilerande effekt
                    spriteX = 16 * 4;
            }

            var firstMagicalPlayerParamNew = new Point((int)((px - ox) * 16.0f), (int)((py - oy) * 16.0f));
            var secondMagicalPlayerParamNew = new Point(spriteX, spriteY);
            graphics.DrawPartialSprite(firstMagicalPlayerParamNew, item.Sprite, secondMagicalPlayerParamNew, 16, 16);

        }

        public override void Update(float elapsedTime, DynamicGameObject player)
        {
            if (this.IsTempEnergi)
            {
                if (Collectable == 16)
                {
                    vx = Core.Aggregate.Instance.RNG(-5, 5);
                    vy = Core.Aggregate.Instance.RNG(-14, -3);

                    Collectable--;
                }
                else if (Collectable > 0)
                {
                   
                    Collectable--;
                }
                else
                {
                    Collectable--;
                    if (Collectable < -64)
                    {
                        RemoveCount = 5;
                    }
                }
            }

        

        }

        public override void OnInteract(DynamicGameObject player = null)
        {
            if (Collected || Collectable > 0)
                return;



            if (item.OnInteract(player))
            {
                // Add item to inventory
                Engine.GiveItem(item);
            }

            Collected = true;
        }
    }
}
