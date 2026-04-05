#nullable enable
using System;
using OlcSideScrollingConsoleGame.Rendering;
using OlcSideScrollingConsoleGame.Systems;

namespace OlcSideScrollingConsoleGame.States
{
    /// <summary>
    /// Samlar alla infrastrukturtjänster som ett spelläge behöver och injiceras
    /// i varje state-konstruktor för att undvika att skicka sju parametrar separat.
    /// </summary>
    /// <remarks>
    /// MÖNSTER: Service Locator (Parameter Object)
    ///
    /// MOTIVERING:
    /// Utan den här klassen skulle varje state-konstruktor behöva 7+ parametrar
    /// (IInputProvider, ICameraSystem, IRenderContext, GameStateManager, tre callbacks).
    /// GameServices paketerar dem och gör konstruktorsignaturer läsbara.
    /// Den är ett rent databärande objekt — ingen logik läggs här.
    ///
    /// ANVÄNDNING:
    /// Skapas en gång i Program.cs (Composition Root). Skickas till varje state
    /// via konstruktorn. Callbacks (ChangeMap, Reset, Load, Save) pekar på metoder
    /// i Program.cs och fångar dess privata fält via closure.
    /// </remarks>
    public class GameServices
    {
        /// <summary>Abstraherat tangentbords- och gamepad-input.</summary>
        public IInputProvider Input { get; }

        /// <summary>Kameraberäkning för scroll-kartor.</summary>
        public ICameraSystem Camera { get; }

        /// <summary>Tile-renderare för kartvisning.</summary>
        public ITileMapRenderer TileRenderer { get; }

        /// <summary>Motor-agnostisk renderingskontekst för all grafik.</summary>
        public IRenderContext RenderContext { get; }

        /// <summary>Statemaskinen — används av states för att göra övergångar.</summary>
        public GameStateManager StateManager { get; }

        /// <summary>
        /// Laddar och aktiverar en karta. Kallar Program.ChangeMap(name, x, y, hero)
        /// med spelarens nuvarande hero-objekt från GameContext.
        /// </summary>
        public Action<string, float, float> ChangeMap { get; }

        /// <summary>
        /// Nollställer spelet (nytt spel). Kallar Program.Reset() som anropar
        /// Clock.HardReset() och återställer GameContext-fält.
        /// </summary>
        public Action Reset { get; }

        /// <summary>Laddar ett sparat spel från angiven slot (1–3).</summary>
        public Action<int> Load { get; }

        /// <summary>Sparar nuvarande spel till angiven slot (1–3).</summary>
        public Action<int> Save { get; }

        /// <summary>
        /// Skapar ett nytt GameServices med alla beroenden och callbacks.
        /// </summary>
        public GameServices(
            IInputProvider input,
            ICameraSystem camera,
            ITileMapRenderer tileRenderer,
            IRenderContext renderContext,
            GameStateManager stateManager,
            Action<string, float, float> changeMap,
            Action reset,
            Action<int> load,
            Action<int> save)
        {
            Input        = input;
            Camera       = camera;
            TileRenderer = tileRenderer;
            RenderContext = renderContext;
            StateManager = stateManager;
            ChangeMap    = changeMap;
            Reset        = reset;
            Load         = load;
            Save         = save;
        }
    }
}
