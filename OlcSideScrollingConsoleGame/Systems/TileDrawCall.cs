namespace OlcSideScrollingConsoleGame.Systems
{
    /// <summary>
    /// Representerar ett enskilt tile-ritningsanrop: var på skärmen
    /// en tile ska ritas och vilken del av spritesheetet som används.
    ///
    /// Innehåller inga PixelEngine-typer — koordinaterna är rena heltal
    /// och enkla att kontrollera i enhetstester.
    /// </summary>
    public struct TileDrawCall
    {
        /// <summary>Pixelposition X på skärmen.</summary>
        public int ScreenX { get; }

        /// <summary>Pixelposition Y på skärmen.</summary>
        public int ScreenY { get; }

        /// <summary>Käll-X i spritesheetet (pixlar).</summary>
        public int SpriteX { get; }

        /// <summary>Käll-Y i spritesheetet (pixlar).</summary>
        public int SpriteY { get; }

        /// <summary>Tile-bredd i pixlar.</summary>
        public int TileWidth { get; }

        /// <summary>Tile-höjd i pixlar.</summary>
        public int TileHeight { get; }

        public TileDrawCall(int screenX, int screenY, int spriteX, int spriteY, int tileWidth, int tileHeight)
        {
            ScreenX    = screenX;
            ScreenY    = screenY;
            SpriteX    = spriteX;
            SpriteY    = spriteY;
            TileWidth  = tileWidth;
            TileHeight = tileHeight;
        }
    }
}
