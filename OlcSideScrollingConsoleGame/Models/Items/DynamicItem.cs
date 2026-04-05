using OlcSideScrollingConsoleGame.Models.Objects;
using OlcSideScrollingConsoleGame.Rendering;
using System;

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
      

        public override void DrawSelf(IRenderContext graphics, float ox, float oy)
        {
            if (Collected)
            {
                this.RemoveCount = 5;
                return;
            }

            int spriteX = 0;
            int spriteY = 0;
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
                else if (Collectable > -60)
                    spriteX = 16 * 4;
            }

            int screenX = (int)((px - ox) * 16.0f);
            int screenY = (int)((py - oy) * 16.0f);
            // Alla spelplockobjekt ritar från SpriteId.Items-arket (ItemEnergi -> "items")
            graphics.DrawPartialSprite(SpriteId.Items, screenX, screenY, spriteX, spriteY, 16, 16);
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
