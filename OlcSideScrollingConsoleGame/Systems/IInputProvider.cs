#nullable enable
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
        bool IsSelectDown     { get; }
        bool IsAnyKeyPressed  { get; }

        // ─────────────────────────────────────────────
        // Hoppknappens tillstånd (komplex logik)
        // ─────────────────────────────────────────────
        int  JumpButtonState           { get; set; }   // Program.cs hanterar dessa tillsammans med InputManager
        bool JumpButtonPressRelease    { get; set; }
        bool JumpButtonDownRelease     { get; set; }
        bool JumpButtonDownReleaseOnce { get; set; }
        int  JumpButtonCounter         { get; set; }

        // ─────────────────────────────────────────────
        // Idle-tillstånd
        // ─────────────────────────────────────────────
        bool IsIdle             { get; }
        bool ButtonsHasGoneIdle { get; set; }
        void ResetIdle();

        // ─────────────────────────────────────────────
        // Fönsterfokus
        // ─────────────────────────────────────────────

        /// <summary>
        /// True om spelfönstret är aktivt och har fokus.
        /// Används för att undvika att hantera input när fönstret är minimerat.
        /// </summary>
        bool IsWindowFocused { get; }

        // ─────────────────────────────────────────────
        // Uppdatering
        // ─────────────────────────────────────────────

        /// <summary>
        /// Uppdaterar gamepad-tillståndet. Kallas en gång per frame.
        /// </summary>
        void Poll();
    }
}
