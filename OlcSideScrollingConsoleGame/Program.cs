
/***************************************************************************************************************************************
* Developer notes                                                                                                                      *
*                                                                                                                                      *                                                                                                                                   *
* This project (Penguin After All) started out as a pure tinker project trying out OLCPixelGameEngine.                                 *
*                                                                                                                                      *
* As the project progressed I started indulging the thought of actually publishing the compiled version to the public.                 *
* The game is available for free on itch.io (https://furynacho.itch.io/penguin-after-all)                                              *
* Later, this project became a tinkering project to test out Claude Code. This gave new life to the project                            *
* and the goal became to refactor and update the project.                                                                              *
*                                                                                                                                      *
* I want to remind you that all creative content belonging to this project is copyright protected.                                     *
*                                                                                                                                      *
* 2026-04-06, Dev.                                                                                                                     *
*                                                                                                                                      *
***************************************************************************************************************************************/

using System;
using System.Collections.Generic;
using PixelEngine;
using OlcSideScrollingConsoleGame.Models.Objects;
using OlcSideScrollingConsoleGame.Models.Items;
using OlcSideScrollingConsoleGame.Commands;
using OlcSideScrollingConsoleGame.Models;
using OlcSideScrollingConsoleGame.Global;
using OlcSideScrollingConsoleGame.Systems;

namespace OlcSideScrollingConsoleGame
{
    /// <summary>
    /// Composition Root — skapar och kopplar samman alla system, startar spelloop.
    /// </summary>
    /// <remarks>
    /// MÖNSTER: Composition Root
    ///
    /// MOTIVERING:
    /// Program.cs var tidigare en 5 000-raders God Object med all spellogik inbäddad.
    /// Efter Fas 4b Steg 5 delegeras all spellogik till GameStateManager och dess
    /// IGameState-implementationer. Program.cs ansvarar nu enbart för att skapa
    /// beroenden, koppla samman systemen och starta spelloopen.
    ///
    /// ANVÄNDNING:
    /// OnCreate sätter upp alla system och registrerar SplashState som startläge.
    /// OnUpdate delegerar till GameStateManager varje frame.
    /// Infrastrukturmetoder (ChangeMap, Reset, Load, Save) injiceras som delegates
    /// i GameServices och anropas av states via _services.
    /// </remarks>
    public class Program : Game
    {
        // ── Infrastruktur ─────────────────────────────────────────────────────
        private IInputProvider          _input;
        private ICameraSystem           _camera;
        private ITileMapRenderer        _tileRenderer;
        private Rendering.PixelEngineRenderContext _renderContext;

        // ── Ny tillståndsmaskin (Fas 4b Steg 5) ──────────────────────────────
        private States.GameStateManager _stateManager;
        private States.GameServices     _services;

        // ── Delad spelkontext (Blackboard) ────────────────────────────────────
        private Core.GameContext _context = new Core.GameContext();

        // ── Skärmkonstanter ───────────────────────────────────────────────────
        const int ScreenW = GameConstants.ScreenWidth;
        const int ScreenH = GameConstants.ScreenHeight;
        const int PixW    = GameConstants.PixelWidth;
        const int PixH    = GameConstants.PixelHeight;
        const int FrameR  = GameConstants.FrameRate;

        // ── Egenskaper som delegerar till _context (används av Reset/Load/Save/ChangeMap) ──
        private DynamicCreatureHero Hero
        {
            get => _context.Player;
            set => _context.Player = value;
        }
        private List<DynamicGameObject> listDynamics => _context.ActiveObjects;

        private OlcSideScrollingConsoleGame.Models.Map CurrentMap
        {
            get => _context.CurrentLevel;
            set => _context.CurrentLevel = value;
        }
        private List<Quest> ListQuests
        {
            get => _context.ActiveQuests;
            set => _context.ActiveQuests = value;
        }
        private List<Item> ListItems
        {
            get => _context.CollectedItems;
            set => _context.CollectedItems = value;
        }
        public List<int> EnergiIdLista
        {
            get => _context.CollectedEnergiIds;
            set => _context.CollectedEnergiIds = value;
        }
        private TimeSpan ActualTotalTime
        {
            get => _context.ActualTotalTime;
            set => _context.ActualTotalTime = value;
        }
        private TimeSpan GameTotalTime => _context.GameTotalTime;

        private bool RightToAccessPodium
        {
            get => _context.RightToAccessPodium;
            set => _context.RightToAccessPodium = value;
        }

        // ── Entry point ───────────────────────────────────────────────────────
        static void Main(string[] args)
        {
            try
            {
                Program prg = new Program();
                prg.PixelMode = Pixel.Mode.Alpha;
                prg.Construct(ScreenW, ScreenH, PixW, PixH, FrameR);
                prg.Start();
            }
            catch (Exception ex)
            {
                Core.Aggregate.Instance.ReadWrite.WriteToLog("main " + ex.ToString());
            }
        }

        // ── Initiering ────────────────────────────────────────────────────────
        public override void OnCreate()
        {
            Core.Aggregate.Instance.Load(this);

            ActualTotalTime = new TimeSpan();
            Hero = new DynamicCreatureHero();
            ChangeMap("worldmap", 2, 3, Hero);

            _input        = new InputManager(this);
            _camera       = new CameraSystem();
            _tileRenderer = new TileMapRenderer();

            _renderContext = new Rendering.PixelEngineRenderContext(this);
            _renderContext.RegisterSprite(Rendering.SpriteId.Font,              Core.Aggregate.Instance.GetSprite("font"));
            _renderContext.RegisterSprite(Rendering.SpriteId.Items,             Core.Aggregate.Instance.GetSprite("items"));
            _renderContext.RegisterSprite(Rendering.SpriteId.Hero,              Core.Aggregate.Instance.GetSprite("hero"));
            _renderContext.RegisterSprite(Rendering.SpriteId.EnemyPenguin,      Core.Aggregate.Instance.GetSprite("enemyone"));
            _renderContext.RegisterSprite(Rendering.SpriteId.EnemyWalrus,       Core.Aggregate.Instance.GetSprite("enemytwo"));
            _renderContext.RegisterSprite(Rendering.SpriteId.EnemyFrost,        Core.Aggregate.Instance.GetSprite("enemythree"));
            _renderContext.RegisterSprite(Rendering.SpriteId.EnemyIcicle,       Core.Aggregate.Instance.GetSprite("enemyzero"));
            _renderContext.RegisterSprite(Rendering.SpriteId.EnemyBoss,         Core.Aggregate.Instance.GetSprite("enemyboss"));
            _renderContext.RegisterSprite(Rendering.SpriteId.EnemyWind,         Core.Aggregate.Instance.GetSprite("enemywind"));
            _renderContext.RegisterSprite(Rendering.SpriteId.WorldMapTileSheet,  Core.Aggregate.Instance.GetSprite("tilesheetwm"));
            _renderContext.RegisterSprite(Rendering.SpriteId.SplashStart,        Core.Aggregate.Instance.GetSprite(Global.GlobalNamespace.SplashScreenRef.Start));
            _renderContext.RegisterSprite(Rendering.SpriteId.SplashEnd,          Core.Aggregate.Instance.GetSprite(Global.GlobalNamespace.SplashScreenRef.End));
            _renderContext.RegisterSprite(Rendering.SpriteId.EndArt,             Core.Aggregate.Instance.GetSprite("endart"));
            _renderContext.RegisterSprite(Rendering.SpriteId.MapTileSheet,       CurrentMap.Sprite);

            _stateManager = new States.GameStateManager();
            _services = new States.GameServices(
                _input, _camera, _tileRenderer, _renderContext, _stateManager,
                new Systems.AudioSystem(Core.Aggregate.Instance.Sound),
                (mapName, x, y) => ChangeMap(mapName, x, y),
                Reset,
                Load,
                Save
            );
            _stateManager.SetInitial(new States.SplashState(_services), _context);
        }

        // ── Spelloop ──────────────────────────────────────────────────────────
        public override void OnUpdate(float elapsed)
        {
            // Uppdatera spelklockan i kontexten så att alla states kan läsa den
            _context.GameTotalTime = Clock.Total + _context.ActualTotalTime;

            // Delegera all uppdatering till tillståndsmaskinen
            _stateManager.Update(_context, elapsed);
        }

        // ── Infrastrukturoperationer (injiceras i GameServices som delegates) ─

        private void Reset()
        {
            Core.Aggregate.Instance.Settings.ActivePlayer = new SaveSlot();
            Hero.Health = Core.Aggregate.Instance.Settings.ActivePlayer.HeroEnergi;
            ActualTotalTime = new TimeSpan();
            Clock.HardReset();
            RightToAccessPodium = true;
            Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = 0;
            EnergiIdLista = new List<int>();
        }

        private void Load(int slot)
        {
            if (slot == 3)
            {
                var copyObj = new SaveSlot()
                {
                    DateTime       = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.DateTime,
                    Time           = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.Time,
                    IsUsed         = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.IsUsed,
                    HeroEnergi     = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.HeroEnergi,
                    StageCompleted = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.StageCompleted,
                    SpawnAtWorldMap = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.SpawnAtWorldMap,
                    ShowEnd        = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.ShowEnd,
                    EnergiCollected = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.EnergiCollected
                };
                ActualTotalTime = copyObj.Time;
                Core.Aggregate.Instance.Settings.ActivePlayer = copyObj;
                Hero.Health = copyObj.HeroEnergi;
                EnergiIdLista = copyObj.EnergiCollected;
            }
            else if (slot == 2)
            {
                var copyObj = new SaveSlot()
                {
                    DateTime       = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.DateTime,
                    Time           = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.Time,
                    IsUsed         = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.IsUsed,
                    HeroEnergi     = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.HeroEnergi,
                    StageCompleted = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.StageCompleted,
                    SpawnAtWorldMap = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.SpawnAtWorldMap,
                    ShowEnd        = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.ShowEnd,
                    EnergiCollected = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.EnergiCollected
                };
                ActualTotalTime = copyObj.Time;
                Core.Aggregate.Instance.Settings.ActivePlayer = copyObj;
                Hero.Health = copyObj.HeroEnergi;
                EnergiIdLista = copyObj.EnergiCollected;
            }
            else
            {
                var copyObj = new SaveSlot()
                {
                    DateTime       = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.DateTime,
                    Time           = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.Time,
                    IsUsed         = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.IsUsed,
                    HeroEnergi     = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.HeroEnergi,
                    StageCompleted = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.StageCompleted,
                    SpawnAtWorldMap = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.SpawnAtWorldMap,
                    ShowEnd        = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.ShowEnd,
                    EnergiCollected = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.EnergiCollected
                };
                ActualTotalTime = copyObj.Time;
                Core.Aggregate.Instance.Settings.ActivePlayer = copyObj;
                Hero.Health = copyObj.HeroEnergi;
                EnergiIdLista = copyObj.EnergiCollected;
            }
            Clock.HardReset();
        }

        private void Save(int slot)
        {
            var copyObj = new SaveSlot()
            {
                DateTime        = Core.Aggregate.Instance.Settings.ActivePlayer.DateTime,
                Time            = Core.Aggregate.Instance.Settings.ActivePlayer.Time,
                IsUsed          = Core.Aggregate.Instance.Settings.ActivePlayer.IsUsed,
                HeroEnergi      = Core.Aggregate.Instance.Settings.ActivePlayer.HeroEnergi,
                StageCompleted  = Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted,
                SpawnAtWorldMap = Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap,
                ShowEnd         = Core.Aggregate.Instance.Settings.ActivePlayer.ShowEnd,
                EnergiCollected = EnergiIdLista
            };

            if (slot == 3)
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree = copyObj;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.Time       = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.DateTime   = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.IsUsed     = true;
            }
            else if (slot == 2)
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo = copyObj;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.Time       = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.DateTime   = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.IsUsed     = true;
            }
            else
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne = copyObj;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.Time       = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.DateTime   = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.IsUsed     = true;
            }
        }

        public void ChangeMap(string MapName, float x, float y)
        {
            // Staten hanterar övergången — här laddar vi bara kartan
            ChangeMap(MapName, x, y, this.Hero);
        }

        public void ChangeMap(string MapName, float x, float y, DynamicGameObject hero)
        {
            listDynamics.Clear();
            listDynamics.Add(hero);
            var map = Core.Aggregate.Instance.GetMap(MapName);
            if (map == null)
            {
                Core.Aggregate.Instance.ReadWrite.WriteToLog($"ChangeMap: kartan '{MapName}' finns inte.");
                throw new ArgumentException($"Kartan '{MapName}' är inte laddad.", nameof(MapName));
            }
            CurrentMap = map;

            // Uppdatera MapTileSheet i renderContexten när kartan byts.
            // _renderContext kan vara null under det första ChangeMap-anropet i OnCreate
            // (innan _renderContext skapats) — i det fallet sker registreringen i OnCreate.
            if (_renderContext != null && CurrentMap.Sprite != null)
                _renderContext.RegisterSprite(Rendering.SpriteId.MapTileSheet, CurrentMap.Sprite);

            hero.px = x;
            hero.py = y;

            CurrentMap.PopulateDynamics(listDynamics);

            foreach (var q in ListQuests)
                q.PopulateDynamics(listDynamics, CurrentMap.Name);
        }

        public void AddQuest(Quest quest)
        {
            // Lägg till quest främst i listan — nyaste quest prioriteras
            var updated = new List<Quest> { quest };
            foreach (var q in ListQuests)
                updated.Add(q);
            ListQuests = updated;
        }

        public bool GiveItem(Item item)
        {
            ListItems.Add(item);
            return true;
        }

        /// <summary>
        /// Anropas av skriptsystemet för att köa en dialogruta.
        /// Dialog-rendering är ett framtida steg (DialogSystem/DialogState).
        /// </summary>
        public void ShowDialog(List<string> listLines)
        {
            // TODO: koppla till DialogSystem när det extraheras (Fas 4b)
            _context.PendingDialog = listLines;
        }
    }
}
