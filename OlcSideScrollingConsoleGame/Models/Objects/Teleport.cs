#nullable enable
using OlcSideScrollingConsoleGame.Rendering;

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

        public override void DrawSelf(IRenderContext graphics, float ox, float oy)
        {
            int f = (int)(((px + 0.5f) - ox) * 16f);
            int s = (int)(((py + 0.5f) - oy) * 16f);
            var color = RenderColor.Random();

            var (fx1, fy1) = Flicker(f, s); graphics.DrawPixel(fx1, fy1, color);
            var (fx2, fy2) = Flicker(f, s); graphics.DrawPixel(fx2, fy2, color);
            var (fx3, fy3) = Flicker(f, s); graphics.DrawPixel(fx3, fy3, color);
        }

        private (int x, int y) Flicker(int cx, int cy)
        {
            int rx = Core.Aggregate.Instance.RNG(cx - 5, cx + 5);
            int ry = Core.Aggregate.Instance.RNG(cy - 5, cy + 5);
            return (rx, ry);
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
