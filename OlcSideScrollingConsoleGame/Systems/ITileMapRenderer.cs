#nullable enable
using System.Collections.Generic;
using OlcSideScrollingConsoleGame.Models;

namespace OlcSideScrollingConsoleGame.Systems
{
    /// <summary>
    /// Kontrakt för tile-kartans renderingslogik.
    ///
    /// Returnerar en sekvens av <see cref="TileDrawCall"/> som
    /// beskriver vad som ska ritas — utan att faktiskt rita något.
    /// Det faktiska ritanropet (DrawPartialSprite) görs av anroparen
    /// som har tillgång till spelmotorn.
    /// </summary>
    public interface ITileMapRenderer
    {
        /// <summary>
        /// Beräknar vilka tiles som är synliga och var de ska ritas.
        /// </summary>
        /// <param name="cam">Kameravyn för aktuell frame.</param>
        /// <param name="map">Kartdata med tile-index och dimensioner.</param>
        /// <returns>Sekvens av ritningsanrop, ett per synlig tile.</returns>
        IEnumerable<TileDrawCall> GetDrawCalls(CameraView cam, IMapData map);
    }
}
