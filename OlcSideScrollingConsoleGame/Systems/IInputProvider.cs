namespace OlcSideScrollingConsoleGame.Systems
{
    /// <summary>
    /// Abstraktion över spelets input-källor (tangentbord + gamepad).
    /// Gör det möjligt att testa all logik som beror på input
    /// utan åtkomst till riktig hårdvara eller PixelEngine.
    ///
    /// Dependency Inversion Principle: högnivåkod (PhysicsSystem,
    /// menysystem m.m.) beror på detta interface – inte på
    /// konkreta SlimDX- eller PixelEngine-klasser.
    /// </summary>
    public interface IInputProvider
    {
        // ─────────────────────────────────────────────
        // Rörelseknappar
        // ─────────────────────────────────────────────
        bool IsRightDown    { get; }
        bool IsLeftDown     { get; }
        bool IsUpDown       { get; }
        bool IsDownDown     { get; }

        bool IsRightPressed { get; }
        bool IsLeftPressed  { get; }
        bool IsUpPressed    { get; }
        bool IsDownPressed  { get; }

        bool IsRightReleased { get; }
        bool IsLeftReleased  { get; }
        bool IsUpReleased    { get; }
        bool IsDownReleased  { get; }

        // ─────────────────────────────────────────────
        // Actionknappar
        // ─────────────────────────────────────────────
        bool IsJumpDown     { get; }
        bool IsJumpPressed  { get; }
        bool IsJumpReleased { get; }

        bool IsConfirmPressed { get; }
        bool IsCancelPressed  { get; }
        bool IsPausePressed   { get; }
        bool IsRunDown        { get; }
        bool IsAnyKeyPressed  { get; }

        // ─────────────────────────────────────────────
        // Hoppknappens tillstånd (komplex logik)
        // ─────────────────────────────────────────────
        int  JumpButtonState           { get; }
        bool JumpButtonDownRelease     { get; set; }
        bool JumpButtonDownReleaseOnce { get; set; }
        int  JumpButtonCounter         { get; }

        // ─────────────────────────────────────────────
        // Idle-tillstånd
        // ─────────────────────────────────────────────
        bool IsIdle             { get; }
        bool ButtonsHasGoneIdle { get; set; }
        void ResetIdle();

        // ─────────────────────────────────────────────
        // Uppdatering
        // ─────────────────────────────────────────────

        /// <summary>
        /// Uppdaterar gamepad-tillståndet. Kallas en gång per frame.
        /// </summary>
        void Poll();
    }
}
