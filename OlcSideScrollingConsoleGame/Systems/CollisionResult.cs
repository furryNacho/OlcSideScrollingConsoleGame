#nullable enable
namespace OlcSideScrollingConsoleGame.Systems
{
    /// <summary>
    /// Resultatet av ett kollisionstest mot kartan för ett objekt.
    /// Innehåller justerade positioner och flaggor för vilka sidor som träffades.
    /// </summary>
    public readonly record struct CollisionResult
    {
        /// <summary>Justerad X-position efter horisontell kollision.</summary>
        public float NewPosX { get; init; }
        /// <summary>Justerad Y-position efter vertikal kollision.</summary>
        public float NewPosY { get; init; }
        /// <summary>Sant om objektet träffade en solid tile horisontellt.</summary>
        public bool HitWall { get; init; }
        /// <summary>Sant om objektet träffade en solid tile uppåt (tak).</summary>
        public bool HitCeiling { get; init; }
        /// <summary>Sant om objektet landade på en solid tile (golvet).</summary>
        public bool Grounded { get; init; }
    }
}
