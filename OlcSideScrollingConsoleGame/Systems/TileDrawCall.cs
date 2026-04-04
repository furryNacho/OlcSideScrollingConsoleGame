#nullable enable
namespace OlcSideScrollingConsoleGame.Systems
{
    /// <summary>
    /// Representerar ett enskilt tile-ritningsanrop: var på skärmen
    /// en tile ska ritas och vilken del av spritesheetet som används.
    ///
    /// Innehåller inga PixelEngine-typer — koordinaterna är rena heltal
    /// och enkla att kontrollera i enhetstester.
    /// </summary>
    public readonly record struct TileDrawCall
    {
        /// <summary>Pixelposition X på skärmen.</summary>
        public int ScreenX { get; init; }

        /// <summary>Pixelposition Y på skärmen.</summary>
        public int ScreenY { get; init; }

        /// <summary>Käll-X i spritesheetet (pixlar).</summary>
        public int SpriteX { get; init; }

        /// <summary>Käll-Y i spritesheetet (pixlar).</summary>
        public int SpriteY { get; init; }

        /// <summary>Tile-bredd i pixlar.</summary>
        public int TileWidth { get; init; }

        /// <summary>Tile-höjd i pixlar.</summary>
        public int TileHeight { get; init; }

        public TileDrawCall(int screenX, int screenY, int spriteX, int spriteY, int tileWidth, int tileHeight)
        {
            ScreenX   = screenX;
            ScreenY   = screenY;
            SpriteX   = spriteX;
            SpriteY   = spriteY;
            TileWidth  = tileWidth;
            TileHeight = tileHeight;
        }
    }
}
