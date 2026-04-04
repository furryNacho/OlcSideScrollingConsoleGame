#nullable enable
using PixelEngine;

namespace OlcSideScrollingConsoleGame.Models.Objects
{
    public class Teleport : DynamicGameObject
    {
        public Teleport(float x, float y, string MapName, float tx, float ty) : base("Teleport")
        {
            px = x;
            py = y;
            this.MapPosX = tx;
            this.MapPosY = ty;
            this.MapName = MapName;
            SolidVsDynamic = false;
            SolidVsMap = false;
        }

     

        public override void DrawSelf(Program graphics, float ox, float oy)
        {
            float fvalue = 16;//8 gör att den typ svävar på rutan. antar att det är det som avgör var skiten ska ritas ut
            float svalue = 16;//8 flyttar upp den en massa, collisionen är fortfarande på samma plats
            float radiusVAlue = 16;//size på cirkel
            // Does nothing
            // för att kunna se
            int f = (int)(((px + 0.5f) - ox) * fvalue);
            int s = (int)(((py + 0.5f) - oy) * svalue);
            var point = new Point(f, s);
            var radius = (int)(radiusVAlue * 0.5f);
            var color = Pixel.Random();

            // graphics.DrawCircle(point, radius, color); 
            graphics.Draw(Flicker(point), color);
            graphics.Draw(Flicker(point), color);
            graphics.Draw(Flicker(point), color);
         
        }

        private Point Flicker(Point c)
        {
            int rx = Core.Aggregate.Instance.RNG((int)c.X - 5, (int)c.X + 5);
            int ry = Core.Aggregate.Instance.RNG((int)c.Y - 5, (int)c.Y + 5);
            return new Point(rx, ry);
        }

       

        public override void Update(float elapsedTime, DynamicGameObject? player = null)
        {
            //does nothing
        }

        public string MapName { get; set; }
        public float MapPosX { get; set; }
        public float MapPosY { get; set; }
    }
}
