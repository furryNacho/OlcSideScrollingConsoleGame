#nullable enable
using OlcSideScrollingConsoleGame.Models;

namespace OlcSideScrollingConsoleGame.Systems
{
    /// <summary>
    /// Hanterar tile-baserad kollisionsdetektering och positionsjustering (SRP).
    /// Alla metoder är rena funktioner — de muterar inga objekt och har inga sidoeffekter.
    /// Spelspecifik logik (fiende-AI, hjälte-state, ljud) hanteras av anroparen.
    /// </summary>
    public static class CollisionSystem
    {
        /// <summary>
        /// Löser horisontell kollision mot kartan.
        /// </summary>
        /// <param name="posY">Nuvarande Y-position för objektet.</param>
        /// <param name="newPosX">Föreslagen ny X-position.</param>
        /// <param name="velX">Nuvarande horisontell hastighet.</param>
        /// <param name="border">Precision för hitbox (CollisionBorderPrecision).</param>
        /// <param name="map">Kartan att testa kollision mot.</param>
        /// <returns>Justerad X-position och om en solid tile träffades.</returns>
        public static (float adjustedX, bool hitWall) ResolveHorizontal(
            float posY, float newPosX, float velX, float border, IMapData map)
        {
            if (velX <= 0) // Moving Left
            {
                if (map.GetSolid((int)(newPosX + 0.0f), (int)(posY + 0.0f)) ||
                    map.GetSolid((int)(newPosX + 0.0f), (int)(posY + 0.9f)))
                {
                    return ((int)(newPosX + 0.9f), true);
                }
            }
            else // Moving Right
            {
                if (map.GetSolid((int)(newPosX + (1.0f - border)), (int)(posY + border + 0.0f)) ||
                    map.GetSolid((int)(newPosX + (1.0f - border)), (int)(posY + (1.0f - border))))
                {
                    return ((int)newPosX, true);
                }
            }

            return (newPosX, false);
        }

        /// <summary>
        /// Löser vertikal kollision mot kartan.
        /// </summary>
        /// <param name="newPosX">Justerad X-position (efter horisontell kollision).</param>
        /// <param name="newPosY">Föreslagen ny Y-position.</param>
        /// <param name="velY">Nuvarande vertikal hastighet.</param>
        /// <param name="map">Kartan att testa kollision mot.</param>
        /// <returns>Justerad Y-position, om taket träffades och om objektet landade.</returns>
        public static (float adjustedY, bool hitCeiling, bool grounded) ResolveVertical(
            float newPosX, float newPosY, float velY, IMapData map)
        {
            if (velY <= 0) // Moving Up
            {
                if (map.GetSolid((int)(newPosX + 0.0f), (int)newPosY) ||
                    map.GetSolid((int)(newPosX + 0.9f), (int)newPosY))
                {
                    return ((int)newPosY + 1, true, false);
                }
                return (newPosY, false, false);
            }
            else // Moving Down
            {
                if (map.GetSolid((int)(newPosX + 0.0f), (int)(newPosY + 1.0f)) ||
                    map.GetSolid((int)(newPosX + 0.9f), (int)(newPosY + 1.0f)))
                {
                    return ((int)newPosY, false, true);
                }
                return (newPosY, false, false);
            }
        }
    }
}
