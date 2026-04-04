#nullable enable
using System.Collections.Generic;
using OlcSideScrollingConsoleGame.Global;
using OlcSideScrollingConsoleGame.Models;

namespace OlcSideScrollingConsoleGame.Systems
{
    /// <summary>
    /// Beräknar vilka tiles som ska ritas för en given kameravy.
    ///
    /// Loopen itererar över alla synliga tiles (plus en extra buffert-tile
    /// i varje riktning för mjuk scrollning) och beräknar skärm- och
    /// spritesheet-koordinater för varje tile.
    ///
    /// Inga PixelEngine-beroenden — returnerar rena <see cref="TileDrawCall"/>-värden
    /// som anroparen skickar vidare till spelmotorns DrawPartialSprite.
    /// </summary>
    public class TileMapRenderer : ITileMapRenderer
    {
        /// <inheritdoc />
        public IEnumerable<TileDrawCall> GetDrawCalls(CameraView cam, IMapData map)
        {
            for (int x = -1; x < cam.VisibleTilesX + 1; x++)
            {
                for (int y = -1; y < cam.VisibleTilesY + 1; y++)
                {
                    int idx = map.GetIndex((int)(x + cam.OffsetX), (int)(y + cam.OffsetY));

                    int sx = idx % GameConstants.TileSheetColumns;
                    int sy = idx / GameConstants.TileSheetColumns;

                    int screenX = (int)(x * cam.TileWidth  - cam.TileOffsetX);
                    int screenY = (int)(y * cam.TileHeight - cam.TileOffsetY);
                    int spriteX = sx * cam.TileWidth;
                    int spriteY = sy * cam.TileHeight;

                    yield return new TileDrawCall(screenX, screenY, spriteX, spriteY, cam.TileWidth, cam.TileHeight);
                }
            }
        }
    }
}
