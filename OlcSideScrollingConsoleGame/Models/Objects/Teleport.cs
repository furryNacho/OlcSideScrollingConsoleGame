using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Does nothing
            // för att kunna se
            int f = (int)(((px + 0.5f) - ox) * 16.0f);
            int s = (int)(((py + 0.5f) - oy) * 16.0f);
            var point = new Point(f, s);
            var radius = (int)(16.0f * 0.5f);
            var color = Pixel.Random();
            graphics.DrawCircle(point, radius, color);
        }

        public override void Update(float elapsedTime, DynamicGameObject player = null)
        {
            //does nothing
        }

        public string MapName { get; set; }
        public float MapPosX { get; set; }
        public float MapPosY { get; set; }
    }
}
