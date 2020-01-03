using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelEngine;
using PixelEngine.Utilities;
using Gamepad.Library;
using OlcSideScrollingConsoleGame.Models.Objects;
using OlcSideScrollingConsoleGame.Models.Items;
using OlcSideScrollingConsoleGame.Commands;
using OlcSideScrollingConsoleGame.Models;
using System.Threading;

namespace OlcSideScrollingConsoleGame
{
    public class Program : Game
    {
        public Program()
        {
            this.AppName = "Penguin After All";
        }

        private StateMachine<Enum.State> Machine { get; set; }
        private Creature Hero { get; set; }
        public SlimDXGamepad SlimDx { get; set; }
        public IsItPressed IIP { get; set; }
        private bool ButtonsHasGoneIdle { get; set; }

        private List<DynamicGameObject> listDynamics { get; set; } = new List<DynamicGameObject>();

        private OlcSideScrollingConsoleGame.Models.Map CurrentMap { get; set; }
        private Sprite SpriteFont { get; set; }
        private Sprite SpriteItems { get; set; }

        // Camera properties
        float CameraPosX { get; set; } = 0.0f;
        float CameraPosY { get; set; } = 0.0f;
        public int IdleCounter { get; set; } = 0;

        private List<string> ListDialogToShow { get; set; }
        private bool doShowDialog = false;
        private float DialogX = 0.0f;
        private float Dialogy = 0.0f;
        private List<Quest> ListQuests { get; set; } = new List<Quest>();
        private List<Item> ListItems { get; set; } = new List<Item>();
        private EnergiRainObject EnergiRainObj { get; set; } = new EnergiRainObject();

        const int ScreenW = 256;
        const int ScreenH = 240;
        const int PixW = 4;
        const int PixH = 4;
        const int FrameR = -1;
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
                Core.Aggregate.Instance.ReadWrite.WriteToLog(ex.ToString());
            }
        }

        public override void OnCreate()
        {
            /*
            this.Enable(Game.Subsystem.Audio);
            Resx.Load((Game)this);
            this.PlaySound(Resx.BgMusic);
            this.Volume = 0.5f;
            this.machine = new StateMachine<HorseRun.Scenes>();
            this.machine.Switch(HorseRun.Scenes.Menu);
            this.Reset();
             */


            this.Machine = new StateMachine<Enum.State>();
            //this.Machine.Switch(Enum.State.GameMap);
            this.Machine.Switch(Enum.State.SplashScreen);


            Core.Aggregate.Instance.Load(this);

            Hero = new DynamicCreatureHero();
            //ChangeMap("mapone", 5, 5, Hero);
            ChangeMap("worldmap", 2, 3, Hero);


            SlimDx = new SlimDXGamepad();
            SlimDx.SetUp();
            IIP = SlimDx.IIP;
            SlimDx.timer_Tick();

            SpriteFont = Core.Aggregate.Instance.GetSprite("font");
            SpriteItems = Core.Aggregate.Instance.GetSprite("items");
        }

        public override void OnUpdate(float elapsed)
        {
            switch (this.Machine.CurrentState)
            {
                case Enum.State.SplashScreen:
                    this.DisplaySplashScreen(elapsed);
                    break;
                case Enum.State.Menu:
                    this.DisplayMenu(elapsed);
                    break;
                case Enum.State.WorldMap:
                    this.DisplayWorldMap(elapsed);
                    break;
                case Enum.State.GameMap:
                    this.DisplayStage(elapsed);
                    break;
                case Enum.State.Pause:
                    this.DisplayPause(elapsed);
                    break;
                case Enum.State.Settings:
                    throw new NotImplementedException();
                    break;
                case Enum.State.GameOver:
                    this.DisplayGameOver(elapsed);
                    break;
                case Enum.State.HighScore:
                    throw new NotImplementedException();
                    //this.DisplayHighScore(elapsed);
                    break;
                case Enum.State.End:
                    this.DisplayEnd(elapsed);
                    break;
            }
        }

        private void DisplayEnd(float elapsed)
        {
            Core.Aggregate.Instance.Script.ProcessCommands(elapsed); 
            this.Clear((Pixel)Pixel.Presets.Black);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            //Input
            if (Focus)
            {

                //Button spam lock  
                if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    ButtonsHasGoneIdle = true;
                }

                if (ButtonsHasGoneIdle && (GetKey(Key.Any).Pressed || !IIP.idle))
                {
                    Core.Aggregate.Instance.Settings.ShowEnd = false;
                    ButtonsHasGoneIdle = false;
                    this.Machine.Switch(Enum.State.WorldMap);
                   // return;
                }
            }

            DrawBigText("End", 4, 4);
            DrawBigText("You are super player", 4, 20);
            DrawBigText("Thank you for playing game", 4, 36);
            DrawBigText("Press any button", 8, 160);
        }

        private void DisplaySplashScreen(float elapsed)
        {
            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            //Input
            if (Focus)
            {
                if (GetKey(Key.Any).Pressed || !IIP.idle)
                {
                    ButtonsHasGoneIdle = false;
                    this.Machine.Switch(Enum.State.Menu);
                }
            }

            DrawBigText("Penguin After All", 4, 4);
            DrawBigText("Press any button", 8, 160);

        }


        //public bool MenuStartHasBeenReleased { get; set; }
        int selectedMenuItem = 1;

        private void DisplayMenu(float elapsed)
        {
            this.Clear((Pixel)Pixel.Presets.Black);
            DrawBigText("Menu", 4, 4);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;


            var menuList = new List<string>();

            //Pause menu or start menu
            if (!Core.Aggregate.Instance.GetSettings().GameHasStarted)
            {
                menuList = new List<string>()
                {
                    "Start New Game",
                    "Load Saved Game",
                    "View High Score",
                    "Exit"
                };
            }
            else
            {
                menuList = new List<string>()
                {
                    "Resume",
                    "Save",
                    "Load Saved Game",
                    "Exit"
                 };
            }

            int i = 0;
            foreach (var menuItem in menuList)
            {
                //int x = i % 4;
                int x = 1;
                int y = i / 1;
                i++;

                var screenPoint = new Point(8 + x * 20, 20 + y * 20);
                var spritePoint = new Point(16, 48);
                if (i == selectedMenuItem)
                {
                    spritePoint = new Point(0, 48);
                }
                DrawPartialSprite(screenPoint, SpriteItems, spritePoint, 16, 16);

                DrawBigText(menuItem, screenPoint.X + 25, screenPoint.Y + 5);

            }

            //Input
            if (Focus)
            {
                //Button spam lock  
                if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    ButtonsHasGoneIdle = true;
                }

                //Up
                if (selectedMenuItem > 1 && (GetKey(Key.Up).Pressed || IIP.up) && ButtonsHasGoneIdle)
                {
                    selectedMenuItem--;
                    ButtonsHasGoneIdle = false;
                }

                //Down
                if (selectedMenuItem < menuList.Count && (GetKey(Key.Down).Pressed || IIP.down) && ButtonsHasGoneIdle)
                {
                    selectedMenuItem++;
                    ButtonsHasGoneIdle = false;
                }

                // Select
                if (ButtonsHasGoneIdle && (GetKey(Key.S).Pressed || IIP.Button6))
                {
                    ButtonsHasGoneIdle = false;

                    string selectedMenuItemString = menuList[selectedMenuItem - 1];
                    switch (selectedMenuItemString)
                    {
                       
                        case "Start New Game":
                            selectedMenuItem = 1;

                            // TODO: spara settings lite snyggare
                            Core.Aggregate.Instance.Settings.GameHasStarted = true;
                            //Core.Aggregate.Instance.SaveSettings(tempSaveSettings);

                            this.Machine.Switch(Enum.State.WorldMap);
                            ButtonsHasGoneIdle = false;
                            break;
                        case "Resume":
                            selectedMenuItem = 1;
                            this.Machine.Switch(Enum.State.WorldMap);
                            ButtonsHasGoneIdle = false;
                            break;
                        case "Save":
                            selectedMenuItem = 1;
                            // TODO code block
                            DrawBigText(menuList[selectedMenuItem - 1], 45, 4);
                            break;
                        case "Load Saved Game":
                            selectedMenuItem = 1;
                            // TODO code block
                            DrawBigText(menuList[selectedMenuItem - 1], 45, 4);
                            break;
                        case "Exit":
                            Core.Aggregate.Instance.ThisGame.Finish();
                            break;
                        case "View High Score":
                            // TODO code block
                            DrawBigText(menuList[selectedMenuItem - 1], 45, 4);
                            break;
                        default:
                            Core.Aggregate.Instance.ReadWrite.WriteToLog("DisplayMenu - Select value : " + selectedMenuItem + ". Default switch");
                            break;
                    }
                }

                // Tillbaka till worldmap
                //if (!MenuStartHasBeenReleased && !IIP.Button7)
                //{
                //    MenuStartHasBeenReleased = true;
                //}
                //if (ButtonsHasGoneIdle && (GetKey(Key.P).Pressed || IIP.Button7))
                //{
                //    this.Machine.Switch(Enum.State.WorldMap);
                //    //WorldmapStartHasBeenReleased = false;
                //    ButtonsHasGoneIdle = false;
                //}
            }

        }



        bool no1 = true;
        bool no2 = false;
        bool no3 = false;
        bool no4 = false;
        bool no5 = false;
        bool no6 = false;
        //int runStageNo = 1;
        bool hasAccumulatedAllSpeed = false;
        //public bool WorldmapStartHasBeenReleased { get; set; }
        private void DisplayWorldMap(float elapsed)
        {
            Core.Aggregate.Instance.Script.ProcessCommands(elapsed); 

            //TODO hantera om slut
            if (Core.Aggregate.Instance.Settings.ShowEnd)
            {
                this.Machine.Switch(Enum.State.End);
                ButtonsHasGoneIdle = false;
                //return;
            }

            this.Clear((Pixel)Pixel.Presets.Black);

            if (!hasAccumulatedAllSpeed)
            {
                hasAccumulatedAllSpeed = true;
                Hero.vx = 0;
                Hero.vy = 0;
            }

            float corrWorldMapPosX = 2;
            float corrWorldMapPosY = 3;

            if (Core.Aggregate.Instance.Settings.SpawnAtWorldMap == 1)
            {
                corrWorldMapPosX = 2;
                corrWorldMapPosY = 3;
            }
            else if (Core.Aggregate.Instance.Settings.SpawnAtWorldMap == 2)
            {
                corrWorldMapPosX = 5;
                corrWorldMapPosY = 3;
            }
            else if (Core.Aggregate.Instance.Settings.SpawnAtWorldMap == 3)
            {
                corrWorldMapPosX = 9;
                corrWorldMapPosY = 3;
            }

            if (CurrentMap.Name != "worldmap")
            {
                //TODO: skicka in en placeholder och läsa settings för att avgöra vart på kartan ska placeras
                ChangeMap("worldmap", corrWorldMapPosX, corrWorldMapPosY, Hero);
                ButtonsHasGoneIdle = false;
            }

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;



            #region Input
            // Handle Input
            if (Focus && Hero.vx == 0 && Hero.vy == 0)
            {
                //Button spam lock  
                if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    ButtonsHasGoneIdle = true;
                }

                //Up
                if (GetKey(Key.Up).Down || IIP.up)
                {
                    Hero.vy = -3.0f;
                }

                //Down
                if (GetKey(Key.Down).Down || IIP.down)
                {
                    Hero.vy = 3.0f;
                }

                //Right
                if (GetKey(Key.Right).Down || IIP.right)
                {
                    Hero.vx = 3;
                }

                //Left
                if (GetKey(Key.Left).Down || IIP.left)
                {
                    Hero.vx = -3;
                }

                // A ("jump button")
                if (ButtonsHasGoneIdle && (GetKey(Key.Space).Pressed || IIP.Button0))
                {
                    // TODO : hantera vilen värld man ska till
                    if (Core.Aggregate.Instance.Settings.SpawnAtWorldMap == 1)
                    {
                       
                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapone", 2, 3, Hero);

                        this.Machine.Switch(Enum.State.GameMap);

                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.SpawnAtWorldMap == 2)
                    {
                      
                        hasAccumulatedAllSpeed = false;
                        ChangeMap("maptwo", 2, 3, Hero);

                        this.Machine.Switch(Enum.State.GameMap);

                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.SpawnAtWorldMap == 3)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapthree", 2, 3, Hero);

                        this.Machine.Switch(Enum.State.GameMap);

                        ButtonsHasGoneIdle = false;

                        return;
                    }

                    //return;
                }


                if (ButtonsHasGoneIdle && (GetKey(Key.P).Pressed || IIP.Button7))
                {
                    // Todo öppna meny
                    this.Machine.Switch(Enum.State.Menu);
                    //MenuStartHasBeenReleased = false;
                    ButtonsHasGoneIdle = false;
                }

            }
            #endregion

            #region Update obj's

            foreach (var myObject in listDynamics)
            {


                if (myObject.IsHero)
                {

                    if ((!no1 || Core.Aggregate.Instance.Settings.StageCompleted == 0) && (myObject.px >= 2 && myObject.px <= 2.05f))
                    {
                        /*Om vx är possitiv - instruktion att gå höger.
                         *Om vx är negativ - instruktion att gå vänster
                         *Om StageCompleted == 0 förbjud att gå till höger specifikt (i detta fall höger)*/
                        if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.StageCompleted != 0)
                        {
                            myObject.vx = 0;
                        }
                        no1 = true;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.SpawnAtWorldMap = 1;

                    }
                    else if ((!no2 || Core.Aggregate.Instance.Settings.StageCompleted == 1) && (myObject.px >= 5 && myObject.px <= 5.09f))
                    {
                        // förbjud att gå höger
                        if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.StageCompleted != 1)
                        {
                            myObject.vx = 0;
                        }
                        no1 = false;
                        no2 = true;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.SpawnAtWorldMap = 2;

                    }
                    else if ((!no3 || Core.Aggregate.Instance.Settings.StageCompleted == 2) && (myObject.px >= 9 && myObject.px <= 9.1f))
                    {
                        // förbjud att gå höger
                        if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.StageCompleted != 2)
                        {
                            myObject.vx = 0;
                        }
                        no1 = false;
                        no2 = false;
                        no3 = true;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.SpawnAtWorldMap = 3;

                    }
                    else if (!no4 && (myObject.px >= 11 && myObject.px <= 11.1f && myObject.py < 3.01))
                    {
                        myObject.vx = 0;
                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = true;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.SpawnAtWorldMap = 0;

                    }
                    else if (!no5 && (myObject.px >= 11 && myObject.py >= 5f && myObject.py <= 5.1f))
                    {
                        myObject.vx = 0;
                        myObject.vy = 0;
                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = true;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.SpawnAtWorldMap = 0;
                    }
                    else if (!no6 && (myObject.px >= 11 && myObject.py >= 7f && myObject.py <= 7.01f))
                    {
                        myObject.vx = 0;
                        myObject.vy = 0;
                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = true;
                        Core.Aggregate.Instance.Settings.SpawnAtWorldMap = 0;
                    }

                }



                float NewObjectPosX = myObject.px + myObject.vx * elapsed;
                float NewObjectPosY = myObject.py + myObject.vy * elapsed;

                //
                // Collision
                //
                float fBorder = 0.000000005f;// Hårdkoda hitbox (bevara för rpg!!)

                if (myObject.vx <= 0) // Moving Left
                {

                    if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.9f)))
                    {
                        NewObjectPosX = (int)NewObjectPosX + 1;
                        myObject.vx = 0;
                    }

                }
                else // Moving Right
                {

                    if (CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + fBorder + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + (1.0f - fBorder))))
                    {
                        if (myObject.IsHero)
                        {
                            NewObjectPosX = (int)NewObjectPosX;
                            myObject.vx = 0;
                        }
                        else
                        {
                            NewObjectPosX = (int)NewObjectPosX;
                            myObject.vx = 0;
                        }
                    }

                }

                myObject.Grounded = false;

                if (myObject.vy <= 0) // Moving Up
                {

                    if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)NewObjectPosY) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)NewObjectPosY))
                    {
                        NewObjectPosY = (int)NewObjectPosY + 1;
                        myObject.vy = 0;
                    }

                }
                else // Moving Down
                {

                    if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(NewObjectPosY + 1.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)(NewObjectPosY + 1.0f)))
                    {
                        NewObjectPosY = (int)NewObjectPosY;
                        myObject.vy = 0;
                        myObject.Grounded = true;
                    }


                }



                //Collision dynamiska objekt
                float DynamicObjectPosX = NewObjectPosX;
                float DynamicObjectPosY = NewObjectPosY;
                foreach (var otherObject in listDynamics)
                {
                    //if (otherObject != myObject && !otherObject.Redundant)
                    if (otherObject != myObject)
                    {

                        if (otherObject.SolidVsDynamic && myObject.SolidVsDynamic)
                        {


                            // Check if bounding rectangles overlap
                            if (DynamicObjectPosX < (otherObject.px + 1.0f) && (DynamicObjectPosX + 1.0f) > otherObject.px &&
                                 myObject.py < (otherObject.py + 1.0f) && (myObject.py + 1.0f) > otherObject.py)
                            //if (DynamicObjectPosX < (otherObject.px + 0.0f) &&
                            //   (DynamicObjectPosX + 0.0f) > otherObject.px &&
                            //    myObject.py < (otherObject.py + 0.0f) &&
                            //   (myObject.py + 0.0f) > otherObject.py)
                            {
                                // First Check Horizontally 
                                if (myObject.vx < 0)
                                {
                                    DynamicObjectPosX = otherObject.px + 1.0f; // (krock från höger)

                                    //skada hero
                                    if (otherObject.Friendly != myObject.Friendly)
                                    {
                                        //if (otherObject.Friendly)
                                        if (otherObject.IsHero)
                                        {

                                            var victim = (Creature)otherObject;
                                            DamageHero((Creature)myObject, victim, "3");
                                        }
                                    }


                                }
                                else
                                {
                                    DynamicObjectPosX = otherObject.px - 1.0f; // (krock från vänster)

                                    //  skada hero
                                    if (otherObject.Friendly != myObject.Friendly)
                                    {
                                        //if (otherObject.Friendly)
                                        if (otherObject.IsHero)
                                        {
                                            var victim = (Creature)otherObject;

                                            DamageHero((Creature)myObject, victim, "2");
                                        }
                                    }



                                }



                            }

                            // Check if bounding rectangles overlap
                            if (DynamicObjectPosX < (otherObject.px + 1.0f) && (DynamicObjectPosX + 1.0f) > otherObject.px &&
                                DynamicObjectPosY < (otherObject.py + 1.0f) && (DynamicObjectPosY + 1.0f) > otherObject.py)
                            {

                                //  Check Vertically 
                                if (myObject.vy < 0)
                                {

                                    DynamicObjectPosY = otherObject.py + 1.0f; // (krock underifrån)

                                    // Kolla om krocken är mellan fiende och hjälte
                                    if (otherObject.Friendly != myObject.Friendly)
                                    {
                                        if (!otherObject.Friendly)
                                        {
                                            if (Hero.px > otherObject.px)
                                            {
                                                var victim = (Creature)otherObject;
                                                DamageHero((Creature)myObject, victim, "1"); // TODO: har nån bugg här när fi landar på hjälte..
                                            }
                                        }
                                        else
                                        {
                                            if (!myObject.IsHero)
                                            {
                                                //studsa hjälten lite
                                                Hero.vy = -8.5f;

                                                JumpDamage((Creature)Hero, (Creature)myObject);
                                            }
                                        }
                                    }


                                }
                                else
                                {
                                    DynamicObjectPosY = otherObject.py - 1.0f; //(krock från ovan)


                                    // Kolla om krocken är mellan fiende och hjälte
                                    if (otherObject.Friendly != myObject.Friendly)
                                    {
                                        if (!otherObject.Friendly) // otherObject är fiende
                                        {
                                            //studsa hjälten lite
                                            Hero.vy = -8.5f;

                                            JumpDamage((Creature)Hero, (Creature)otherObject);
                                        }
                                    }



                                }
                            }
                        }
                        else
                        {
                            if (myObject.IsHero)
                            {
                                // Object is player and can interact with things
                                if (DynamicObjectPosX < (otherObject.px + 1.0f) && (DynamicObjectPosX + 1.0f) > otherObject.px &&
                                    myObject.py < (otherObject.py + 1.0f) && (myObject.py + 1.0f) > otherObject.py)
                                {
                                    //// First check if object is part of a quest
                                    //foreach (var quest in listQuests)
                                    //{
                                    //    if (quest.OnInteraction(listDynamics, otherObject, Enum.NATURE.WALK))
                                    //        break;
                                    //}

                                    // Then check if it is map related
                                    CurrentMap.OnInteraction(listDynamics, otherObject, Enum.NATURE.WALK);

                                    // Finally just check the object
                                    otherObject.OnInteract(myObject);
                                }
                            }
                        }
                    }
                }



                // Apply new position
                myObject.px = DynamicObjectPosX;
                myObject.py = DynamicObjectPosY;


                //Uppdatera Objektet!
                myObject.Update(elapsed, Hero);

            }

            #endregion

            #region Draw lvl

            //TODO slå ihop
            CameraPosX = 0;
            CameraPosY = 0;

            // Draw Levels
            int nTileWidth = 16;
            int nTileHeight = 16;
            int nVisibleTilesX = ScreenWidth / nTileWidth;
            int nVisibleTilesY = ScreenHeight / nTileHeight;

            // Calculate Top-Leftmost visible tile
            float fOffsetX = CameraPosX - (float)nVisibleTilesX / 2.0f;
            float fOffsetY = CameraPosY - (float)nVisibleTilesY / 2.0f;

            // Clamp camera to game boundaries
            if (fOffsetX < 0) fOffsetX = 0;
            if (fOffsetY < 0) fOffsetY = 0;

            if (fOffsetX > CurrentMap.Width - nVisibleTilesX) fOffsetX = CurrentMap.Width - nVisibleTilesX;
            if (fOffsetY > CurrentMap.Height - nVisibleTilesY) fOffsetY = CurrentMap.Height - nVisibleTilesY;



            // Get offsets for smooth movement
            float fTileOffsetX = (fOffsetX - (int)fOffsetX) * nTileWidth;
            float fTileOffsetY = (fOffsetY - (int)fOffsetY) * nTileHeight;

            // Draw visible tile map
            for (int x = -1; x < nVisibleTilesX + 1; x++)
            {
                for (int y = -1; y < nVisibleTilesY + 1; y++)
                {

                    PixelEngine.Point firstMagicalParam = new Point();
                    PixelEngine.Point secondMagicalParam = new Point();

                    int idx = CurrentMap.GetIndex((int)(x + fOffsetX), (int)(y + fOffsetY));

                    int sx = idx % 5; //column that the sprite is on  // TODO: hårt nummer && tror det 'r antal tiles som finns i spriten
                    int sy = idx / 5; // row that the tile is on  // TODO: hårt nummer 

                    var firstMagicalPlayerParamNew = new Point((int)(x * nTileWidth - fTileOffsetX), (int)(y * nTileHeight - fTileOffsetY));
                    var secondMagicalPlayerParamNew = new Point((int)(sx * nTileWidth), (int)(sy * nTileWidth));
                    DrawPartialSprite(firstMagicalPlayerParamNew, CurrentMap.Sprite, secondMagicalPlayerParamNew, nTileWidth, nTileHeight);





                }
            }
            //
            #endregion

            #region Draw obj's
            // Draw all objekts
            foreach (var myObject in listDynamics)
            {
                myObject.DrawSelf(this, fOffsetX, fOffsetY);
            }
            #endregion

            DrawBigText("World map", 25, 25);
            DrawHUD();

            string msg = "player - x: " + Hero.px + " y: " + Hero.py;
            DisplayDialog(new List<string>() { msg }, 10, 10);
        }

        //public bool PauseStartHasBeenReleased { get; set; }
        private void DisplayPause(float elapsed)
        {
            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            //Input
            if (Focus)
            {
                //Button spam lock  
                if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    ButtonsHasGoneIdle = true;
                }

                // Press select - Change to worldmap
                if (GetKey(Key.S).Pressed || IIP.Button6)
                {
                    // TODO: only if stage is done
                    ButtonsHasGoneIdle = false;
                    this.Machine.Switch(Enum.State.WorldMap);
                }

                if (ButtonsHasGoneIdle && (GetKey(Key.P).Pressed || IIP.Button7))
                {
                    // GameStartHasBeenReleased = false;
                    ButtonsHasGoneIdle = false;
                    this.Machine.Switch(Enum.State.GameMap);
                }
            }

            DrawBigText("Pause", 25, 25);
            DrawHUD();
        }

        #region DisplayGameMap
        // public bool GameStartHasBeenReleased { get; set; }
        private void DisplayStage(float elapsed)
        {

            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);

            if (Hero.Health < 1)
            {
                this.Machine.Switch(Enum.State.GameOver);
            }

            //listDynamics = listDynamics.Where(x => x.Redundant == false).ToList(); // Kanske ska dumpa denna för att kunna göra en poff på fiende om jump damage
            listDynamics = listDynamics.Where(x => x.RemoveCount < 4).ToList();

            if (listDynamics == null || listDynamics.Count <= 0)
            {
                throw new Exception();
            }


            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;


            // Handle Input
            if (Focus)
            {
                //Up
                if (GetKey(Key.Up).Down || IIP.up)
                {
                    //Hero.vy = -6.0f;
                    var qwer = (DynamicCreatureHero)Hero;
                    qwer.LookUp = true;
                }
                else
                {
                    var qwer = (DynamicCreatureHero)Hero;
                    qwer.LookUp = false;
                }

                //Down
                if (GetKey(Key.Down).Down || IIP.down)
                {
                    //Hero.vy = 6.0f;
                    var qwer = (DynamicCreatureHero)Hero;
                    qwer.LookDown = true;
                }
                else
                {
                    var qwer = (DynamicCreatureHero)Hero;
                    qwer.LookDown = false;
                }

                //Right
                if (GetKey(Key.Right).Down || IIP.right)
                {
                    Hero.vx += (Hero.Grounded ? 25.0f : 15.0f) * elapsed;
                }

                //Left
                if (GetKey(Key.Left).Down || IIP.left)
                {
                    Hero.vx += (Hero.Grounded ? -25.0f : -15.0f) * elapsed;
                }

                //Jump 
                if (GetKey(Key.Space).Pressed || IIP.Button0)
                {
                    if (Hero.vy == 0)
                    {
                        Hero.vy = -9.0f;
                    }
                }

                //if (GetKey(Key.R).Released || IIP.Button1 || IIP.Button3)
                //{
                //    this.Reset();
                //}

                // Pause
                //Button spam lock  
                if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    ButtonsHasGoneIdle = true;
                }
                if (ButtonsHasGoneIdle && (GetKey(Key.P).Pressed || IIP.Button7))
                {
                    //PauseStartHasBeenReleased = false;
                    ButtonsHasGoneIdle = false;
                    this.Machine.Switch(Enum.State.Pause);
                }


                if (IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    IdleCounter++;
                    if (IdleCounter > 200)
                    {
                        Hero.IsIdle = true;
                        if (IdleCounter > 220 && IdleCounter < 245)
                        {
                            // Anti gravity
                            Hero.vy -= 20.1f * elapsed;
                        }
                    }
                    if (IdleCounter > 250)
                    {
                        IdleCounter = 0;
                        Hero.IsIdle = false;
                    }
                }
                else
                {
                    IdleCounter = 0;
                    if (Hero.IsIdle)
                    {
                        Hero.IsIdle = false;
                    }
                }



            }


            foreach (var myObject in listDynamics)
            {


                if (!myObject.Redundant)
                {

                    // Gravity
                    myObject.vy += 20.0f * elapsed;


                    // Drag
                    if (myObject.IsHero && myObject.Grounded)
                    {
                        myObject.vx += -3.0f * myObject.vx * elapsed;
                        if (Math.Abs(myObject.vx) < 0.01f)
                            myObject.vx = 0.0f;
                    }

                    // Clamp velocities
                    if (myObject.vx > 10.0f)
                        myObject.vx = 10.0f;

                    if (myObject.vx < -10.0f)
                        myObject.vx = -10.0f;

                    if (myObject.vy > 100.0f)
                        myObject.vy = 100.0f;

                    if (myObject.vy < -100.0f)
                        myObject.vy = -100.0f;


                    float NewObjectPosX = myObject.px + myObject.vx * elapsed;
                    float NewObjectPosY = myObject.py + myObject.vy * elapsed;

                    //
                    // Collision
                    //
                    float fBorder = 0.000000005f;// Hårdkoda hitbox (bevara för rpg!!)

                    if (myObject.vx <= 0) // Moving Left
                    {

                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.9f)))
                        {
                            NewObjectPosX = (int)NewObjectPosX + 1;
                            myObject.vx = 0;
                        }

                    }
                    else // Moving Right
                    {

                        if (CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + fBorder + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + (1.0f - fBorder))))
                        {
                            if (myObject.IsHero)
                            {
                                NewObjectPosX = (int)NewObjectPosX;
                                myObject.vx = 0;
                            }
                            else
                            {
                                NewObjectPosX = (int)NewObjectPosX;
                                myObject.vx = 0;
                            }
                        }

                    }

                    myObject.Grounded = false;

                    if (myObject.vy <= 0) // Moving Up
                    {

                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)NewObjectPosY) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)NewObjectPosY))
                        {
                            NewObjectPosY = (int)NewObjectPosY + 1;
                            myObject.vy = 0;
                        }

                    }
                    else // Moving Down
                    {

                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(NewObjectPosY + 1.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)(NewObjectPosY + 1.0f)))
                        {
                            NewObjectPosY = (int)NewObjectPosY;
                            myObject.vy = 0;
                            myObject.Grounded = true;
                        }


                    }



                    //Collision dynamiska objekt
                    float DynamicObjectPosX = NewObjectPosX;
                    float DynamicObjectPosY = NewObjectPosY;
                    foreach (var otherObject in listDynamics)
                    {
                        //if (otherObject != myObject && !otherObject.Redundant)
                        if (otherObject != myObject)
                        {

                            if (otherObject.SolidVsDynamic && myObject.SolidVsDynamic)
                            {


                                // Check if bounding rectangles overlap
                                if (DynamicObjectPosX < (otherObject.px + 1.0f) && (DynamicObjectPosX + 1.0f) > otherObject.px &&
                                     myObject.py < (otherObject.py + 1.0f) && (myObject.py + 1.0f) > otherObject.py)
                                //if (DynamicObjectPosX < (otherObject.px + 0.0f) &&
                                //   (DynamicObjectPosX + 0.0f) > otherObject.px &&
                                //    myObject.py < (otherObject.py + 0.0f) &&
                                //   (myObject.py + 0.0f) > otherObject.py)
                                {
                                    // First Check Horizontally 
                                    if (myObject.vx < 0)
                                    {
                                        DynamicObjectPosX = otherObject.px + 1.0f; // (krock från höger)

                                        //skada hero
                                        if (otherObject.Friendly != myObject.Friendly)
                                        {
                                            //if (otherObject.Friendly)
                                            if (otherObject.IsHero)
                                            {

                                                var victim = (Creature)otherObject;
                                                DamageHero((Creature)myObject, victim, "3");
                                            }
                                        }


                                    }
                                    else
                                    {
                                        DynamicObjectPosX = otherObject.px - 1.0f; // (krock från vänster)

                                        //  skada hero
                                        if (otherObject.Friendly != myObject.Friendly)
                                        {
                                            //if (otherObject.Friendly)
                                            if (otherObject.IsHero)
                                            {
                                                var victim = (Creature)otherObject;

                                                DamageHero((Creature)myObject, victim, "2");
                                            }
                                        }



                                    }



                                }

                                // Check if bounding rectangles overlap
                                if (DynamicObjectPosX < (otherObject.px + 1.0f) && (DynamicObjectPosX + 1.0f) > otherObject.px &&
                                    DynamicObjectPosY < (otherObject.py + 1.0f) && (DynamicObjectPosY + 1.0f) > otherObject.py)
                                {

                                    //  Check Vertically 
                                    if (myObject.vy < 0)
                                    {

                                        DynamicObjectPosY = otherObject.py + 1.0f; // (krock underifrån)

                                        // Kolla om krocken är mellan fiende och hjälte
                                        if (otherObject.Friendly != myObject.Friendly)
                                        {
                                            if (!otherObject.Friendly)
                                            {
                                                if (Hero.px > otherObject.px)
                                                {
                                                    var victim = (Creature)otherObject;
                                                    DamageHero((Creature)myObject, victim, "1"); // TODO: har nån bugg här när fi landar på hjälte..
                                                }
                                            }
                                            else
                                            {
                                                if (!myObject.IsHero)
                                                {
                                                    //studsa hjälten lite
                                                    Hero.vy = -8.5f;

                                                    JumpDamage((Creature)Hero, (Creature)myObject);
                                                }
                                            }
                                        }


                                    }
                                    else
                                    {
                                        DynamicObjectPosY = otherObject.py - 1.0f; //(krock från ovan)


                                        // Kolla om krocken är mellan fiende och hjälte
                                        if (otherObject.Friendly != myObject.Friendly)
                                        {
                                            if (!otherObject.Friendly) // otherObject är fiende
                                            {
                                                //studsa hjälten lite
                                                Hero.vy = -8.5f;

                                                JumpDamage((Creature)Hero, (Creature)otherObject);
                                            }
                                        }



                                    }
                                }
                            }
                            else
                            {
                                if (myObject.IsHero)
                                {
                                    // Object is player and can interact with things
                                    if (DynamicObjectPosX < (otherObject.px + 1.0f) && (DynamicObjectPosX + 1.0f) > otherObject.px &&
                                        myObject.py < (otherObject.py + 1.0f) && (myObject.py + 1.0f) > otherObject.py)
                                    {
                                        //// First check if object is part of a quest
                                        //foreach (var quest in listQuests)
                                        //{
                                        //    if (quest.OnInteraction(listDynamics, otherObject, Enum.NATURE.WALK))
                                        //        break;
                                        //}

                                        // Then check if it is map related
                                        CurrentMap.OnInteraction(listDynamics, otherObject, Enum.NATURE.WALK);

                                        // Finally just check the object
                                        otherObject.OnInteract(myObject);
                                    }
                                }
                            }
                        }
                    }



                    // Apply new position
                    myObject.px = DynamicObjectPosX;
                    myObject.py = DynamicObjectPosY;


                    //Uppdatera Objektet!
                    myObject.Update(elapsed, Hero);
                }
                else
                {
                    myObject.RemoveCount += 1;
                    myObject.Update(elapsed, Hero);
                }
            }
            // end foreach

            // Hero takes damage, cascade energi
            if (EnergiRainObj.MakeItRain)
            {
                MakeItRainEnergi();
            }


            //// Link camera to player position
            CameraPosX = Hero.px; // Ganska bra om det finns direkt tillgång till spelar obj, även om det kommer finnas massa olika obj, för kameran vill alltid följa spelaren.
            CameraPosY = Hero.py;

            // Draw Levels
            int nTileWidth = 16;
            int nTileHeight = 16;
            int nVisibleTilesX = ScreenWidth / nTileWidth;
            int nVisibleTilesY = ScreenHeight / nTileHeight;

            // Calculate Top-Leftmost visible tile
            float fOffsetX = CameraPosX - (float)nVisibleTilesX / 2.0f;
            float fOffsetY = CameraPosY - (float)nVisibleTilesY / 2.0f;

            // Clamp camera to game boundaries
            if (fOffsetX < 0) fOffsetX = 0;
            if (fOffsetY < 0) fOffsetY = 0;

            if (fOffsetX > CurrentMap.Width - nVisibleTilesX) fOffsetX = CurrentMap.Width - nVisibleTilesX;
            if (fOffsetY > CurrentMap.Height - nVisibleTilesY) fOffsetY = CurrentMap.Height - nVisibleTilesY;



            // Get offsets for smooth movement
            float fTileOffsetX = (fOffsetX - (int)fOffsetX) * nTileWidth;
            float fTileOffsetY = (fOffsetY - (int)fOffsetY) * nTileHeight;

            // Draw visible tile map
            for (int x = -1; x < nVisibleTilesX + 1; x++)
            {
                for (int y = -1; y < nVisibleTilesY + 1; y++)
                {

                    PixelEngine.Point firstMagicalParam = new Point();
                    PixelEngine.Point secondMagicalParam = new Point();

                    int idx = CurrentMap.GetIndex((int)(x + fOffsetX), (int)(y + fOffsetY));

                    int sx = idx % 5; //column that the sprite is on  // TODO: hårt nummer && tror det 'r antal tiles som finns i spriten
                    int sy = idx / 5; // row that the tile is on  // TODO: hårt nummer 

                    var firstMagicalPlayerParamNew = new Point((int)(x * nTileWidth - fTileOffsetX), (int)(y * nTileHeight - fTileOffsetY));
                    var secondMagicalPlayerParamNew = new Point((int)(sx * nTileWidth), (int)(sy * nTileWidth));
                    DrawPartialSprite(firstMagicalPlayerParamNew, CurrentMap.Sprite, secondMagicalPlayerParamNew, nTileWidth, nTileHeight);

                }
            }


            // Draw all objekts
            foreach (var myObject in listDynamics)
            {
                myObject.DrawSelf(this, fOffsetX, fOffsetY);
            }


            // Draw health to screen
            //string Health ="HP: " + Hero.Health.ToString() + "/"+ Hero.MaxHealth;
            //DisplayDialog(new List<string>() { Health }, 160, 10);
            string msg = "player - x: " + Hero.px + " y: " + Hero.py;
            DisplayDialog(new List<string>() { msg }, 10, 10);

            DrawHUD();
        }
        #endregion
        public void DisplayDialog(List<string> listText, int x, int y)
        {
            int nMaxLineLength = 0;
            int nLines = listText.Count;

            foreach (var l in listText)
            {
                if (l.Length > nMaxLineLength)
                {
                    nMaxLineLength = l.Length; // ta ut den längsta raden för att sätta BREDD PÅ RUTAN
                }
            }

            var point = new Point((x - 1), (y - 1));
            var colorA = Pixel.FromRgb((uint)Pixel.Presets.DarkGrey);
            //FillRect(point, (nMaxLineLength * 8 + 1), (y + nLines * 8 + 1), colorA);
            FillRect(point, (nMaxLineLength * 8 + 2), (y + nLines), colorA);

            var point1 = new Point((x - 2), (y - 2));
            var point2 = new Point((x - 2), (y + nLines * 8 + 1));
            var color = Pixel.FromRgb((uint)Pixel.Presets.White);
            DrawLine(point1, point2, color);

            var point3 = new Point((x + nMaxLineLength * 8 + 1), (y - 2));
            var point4 = new Point((x + nMaxLineLength * 8 + 1), (y + nLines * 8 + 1));
            DrawLine(point3, point4, color);

            var point5 = new Point((x - 2), (y - 2));
            var point6 = new Point((x + nMaxLineLength * 8 + 1), (y - 2));
            DrawLine(point5, point6, color);

            var point7 = new Point((x - 2), (y + nLines * 8 + 1));
            var point8 = new Point((x + nMaxLineLength * 8 + 1), (y + nLines * 8 + 1));
            DrawLine(point7, point8, color);

            for (int i = 0; i < listText.Count; i++)
            {
                //DrawBigText(listText[i], x, y + 1 * 8);
                DrawBigText(listText[i], x, y);
            }

        }


        #region Game Over
        private int AnimateCirkle = ScreenH;
        private int AnimationCount = 10;
        private void DisplayGameOver(float elapsed)
        {
            this.Clear((Pixel)Pixel.Presets.Black);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            // ge tillbaka lite liv
            if (Hero.Health < 1)
            {
                Hero.Health = 10;
            }

            // klicka ut ur animation
            if (GetKey(Key.Space).Pressed || IIP.Button0)
            {
                this.Machine.Switch(Enum.State.GameMap);
                AnimateCirkle = ScreenH;
                AnimationCount = 10;
            }
            var color = Pixel.Random();

            if (AnimationCount == 10 || AnimationCount == 9)
            {
                List<Point> listPoint = new List<Point>();
                // Horizontala 
                var point1 = new Point((ScreenW / 2) - 75, (ScreenH / 2) - 1); //start 
                var point2 = new Point((ScreenW / 2) + 75, (ScreenH / 2) - 1); //end
                //DrawLine(point1, point2, color);

                var point3 = new Point((ScreenW / 2) - 150, (ScreenH / 2) + 0); //start
                var point4 = new Point((ScreenW / 2) + 150, (ScreenH / 2) + 0); //end
                //DrawLine(point3, point4, color);

                var point5 = new Point((ScreenW / 2) - 60, (ScreenH / 2) + 1); //start
                var point6 = new Point((ScreenW / 2) + 60, (ScreenH / 2) + 1); //end
                //DrawLine(point5, point6, color);

                var pointArray = new Point[6];
                pointArray = listPoint.ToArray();
                listPoint.Add(point1);
                listPoint.Add(point2);
                listPoint.Add(point3);
                listPoint.Add(point4);
                listPoint.Add(point5);
                listPoint.Add(point6);
                DrawPath(pointArray, color);


            }
            else if (AnimationCount == 8 || AnimationCount == 7)
            {
                // Horizontala 
                var point1 = new Point((ScreenW / 2) - 65, (ScreenH / 2) - 1); //start
                var point2 = new Point((ScreenW / 2) + 65, (ScreenH / 2) - 1); //end
                DrawLine(point1, point2, color);

                var point3 = new Point((ScreenW / 2) - 110, (ScreenH / 2) + 0); //start
                var point4 = new Point((ScreenW / 2) + 110, (ScreenH / 2) + 0); //end
                DrawLine(point3, point4, color);

                var point5 = new Point((ScreenW / 2) - 50, (ScreenH / 2) + 1); //start
                var point6 = new Point((ScreenW / 2) + 50, (ScreenH / 2) + 1); //end
                DrawLine(point5, point6, color);

            }
            else if (AnimationCount == 6 || AnimationCount == 5)
            {
                // Horizontala 
                var point1 = new Point((ScreenW / 2) - 45, (ScreenH / 2) - 1); //start
                var point2 = new Point((ScreenW / 2) + 45, (ScreenH / 2) - 1); //end
                DrawLine(point1, point2, color);

                var point3 = new Point((ScreenW / 2) - 110, (ScreenH / 2) + 0); //start
                var point4 = new Point((ScreenW / 2) + 110, (ScreenH / 2) + 0); //end
                DrawLine(point3, point4, color);

                var point5 = new Point((ScreenW / 2) - 50, (ScreenH / 2) + 1); //start
                var point6 = new Point((ScreenW / 2) + 50, (ScreenH / 2) + 1); //end
                DrawLine(point5, point6, color);


            }
            else if (AnimationCount == 4 || AnimationCount == 3)
            {
                // Horizontala 
                var point1 = new Point((ScreenW / 2) - 15, (ScreenH / 2) - 1); //start
                var point2 = new Point((ScreenW / 2) + 15, (ScreenH / 2) - 1); //end
                DrawLine(point1, point2, color);

                var point3 = new Point((ScreenW / 2) - 100, (ScreenH / 2) + 0); //start
                var point4 = new Point((ScreenW / 2) + 100, (ScreenH / 2) + 0); //end
                DrawLine(point3, point4, color);

                var point5 = new Point((ScreenW / 2) - 20, (ScreenH / 2) + 1); //start
                var point6 = new Point((ScreenW / 2) + 20, (ScreenH / 2) + 1); //end
                DrawLine(point5, point6, color);


            }
            else if (AnimationCount == 2 || AnimationCount == 1)
            {
                // Horizontala 
                var point1 = new Point((ScreenW / 2) - 5, (ScreenH / 2) - 1); //start
                var point2 = new Point((ScreenW / 2) + 5, (ScreenH / 2) - 1); //end
                DrawLine(point1, point2, color);

                var point3 = new Point((ScreenW / 2) - 90, (ScreenH / 2) + 0); //start
                var point4 = new Point((ScreenW / 2) + 90, (ScreenH / 2) + 0); //end
                DrawLine(point3, point4, color);

                var point5 = new Point((ScreenW / 2) - 2, (ScreenH / 2) + 1); //start
                var point6 = new Point((ScreenW / 2) + 2, (ScreenH / 2) + 1); //end
                DrawLine(point5, point6, color);


            }
            else
            {
                // statisk bild
                this.Clear((Pixel)Pixel.Presets.Black);
                DrawBigText("Player Dead", 4, 4);
                DrawBigText("Press button to continue", 8, 160);
            }

            AnimationCount--;
        }
        #endregion

        #region Reset
        //private void Reset()
        //{
        //    var fakeTeleport = listDynamics.FirstOrDefault(x => x.Name == "Teleport");
        //    CurrentMap.OnInteraction(listDynamics, fakeTeleport, Enum.NATURE.WALK);
        //}
        #endregion

        void DrawHUD()
        {
            // Background rec
            var point = new Point(2, 2);
            //var colorBlack = Pixel.FromRgb((uint)Pixel.Presets.DarkBlue);
            var colorBlack = Pixel.FromRgb((uint)Pixel.Presets.Brown);
            var recW = ScreenW - 4;
            FillRect(point, recW, 7, colorBlack);


            // Sprite powerbar
            int WorldX = 3;
            int WorldY = 3;
            var WorldPoint = new Point(WorldX, WorldY);

            int SpriteX = 0;
            int SpriteY = 16 * 4;
            var SpritePoint = new Point(SpriteX, SpriteY);

            DrawPartialSprite(WorldPoint, SpriteItems, SpritePoint, 16, 4);

            // draw some text
            // Should have used ascii.. now this is what I get
            var fullTime = Clock.Total.ToString();
            //int idx = fullTime.LastIndexOf('.');
            //string Text = Hero.Health.ToString() + "% "+ fullTime.Substring(0, idx+2);
            string Text = Hero.Health.ToString() + "% " + fullTime;
            int i = 0;
            foreach (var c in Text)
            {
                int sx = ((c - 32) % 16) * 8;
                int sy = 0;
                if (sx > 72)
                {
                    sx = 72;
                    sy = 69;
                }
                else
                {
                    sy = (((c - 32) / 16) * 8) == 8 ? 75 : 69;
                }

                var onScreenPos = new Point((30 + i * 5), 3);
                var SpriteSheetPos = new Point(sx, sy);

                DrawPartialSprite(onScreenPos, SpriteItems, SpriteSheetPos, 8, 5);

                i++;
            }

            // powerbar meter
            Pixel color = (Pixel)Pixel.Presets.DarkGreen;

            int EnergiOmeter = 14; // 14 är full. Max liv är 99. 
            //int RoundEnergiOmeter = 99 / Hero.Health;
            int RoundEnergiOmeter = Hero.Health;

            if (RoundEnergiOmeter >= 0 && RoundEnergiOmeter < 7)
            {
                EnergiOmeter = 1;
                color = (Pixel)Pixel.Presets.DarkRed;
            }
            else if (RoundEnergiOmeter >= 7 && RoundEnergiOmeter < 14)
            {
                EnergiOmeter = 2;
                color = (Pixel)Pixel.Presets.Yellow;
            }
            else if (RoundEnergiOmeter >= 14 && RoundEnergiOmeter < 21)
            {
                EnergiOmeter = 3;
                color = (Pixel)Pixel.Presets.Yellow;
            }
            else if (RoundEnergiOmeter >= 21 && RoundEnergiOmeter < 28)
            {
                EnergiOmeter = 4;
                color = (Pixel)Pixel.Presets.Green;
            }
            else if (RoundEnergiOmeter >= 28 && RoundEnergiOmeter < 35)
            {
                EnergiOmeter = 5;
                color = (Pixel)Pixel.Presets.Green;
            }
            else if (RoundEnergiOmeter >= 35 && RoundEnergiOmeter < 42)
            {
                EnergiOmeter = 6;
                color = (Pixel)Pixel.Presets.Green;
            }
            else if (RoundEnergiOmeter >= 42 && RoundEnergiOmeter < 49)
            {
                EnergiOmeter = 7;
            }
            else if (RoundEnergiOmeter >= 49 && RoundEnergiOmeter < 56)
            {
                EnergiOmeter = 8;
            }
            else if (RoundEnergiOmeter >= 56 && RoundEnergiOmeter < 62)
            {
                EnergiOmeter = 9;
            }
            else if (RoundEnergiOmeter >= 62 && RoundEnergiOmeter < 69)
            {
                EnergiOmeter = 10;
            }
            else if (RoundEnergiOmeter >= 69 && RoundEnergiOmeter < 76)
            {
                EnergiOmeter = 11;
            }
            else if (RoundEnergiOmeter >= 76 && RoundEnergiOmeter < 82)
            {
                EnergiOmeter = 12;
            }
            else if (RoundEnergiOmeter >= 82 && RoundEnergiOmeter < 89)
            {
                EnergiOmeter = 13;
            }
            else if (RoundEnergiOmeter >= 89 && RoundEnergiOmeter < 96)
            {
                EnergiOmeter = 14;
            }

            var point5 = new Point(WorldX + 1, WorldY + 1); //start
            var point6 = new Point(WorldX + EnergiOmeter, WorldY + 1); //end
            DrawLine(point5, point6, color);

            var point7 = new Point(WorldX + 1, WorldY + 2); //start
            var point8 = new Point(WorldX + EnergiOmeter, WorldY + 2); //end
            DrawLine(point7, point8, color);

        }

        void DrawBigText(string sText, int x, int y)
        {
            int i = 0;
            foreach (var c in sText)
            {
                int sx = ((c - 32) % 16) * 8;
                int sy = ((c - 32) / 16) * 8;

                var firstMagicalPlayerParam = new Point((x + i * 8), y);
                var secondMagicalPlayerParam = new Point(sx, sy);

                DrawPartialSprite(firstMagicalPlayerParam, SpriteFont, secondMagicalPlayerParam, 8, 8);

                i++;
            }
        }

        public void MakeItRainEnergi()
        {

            EnergiRainObj.MakeItRain = false;


            var startX = EnergiRainObj.StartPosX;
            var startY = EnergiRainObj.StartPosY;
            int whenToBeCollectable = 16;

            //Some RNG to determine number of energi to cascade
            int minNumEnergi = EnergiRainObj.NumberOfEnergi / 2 >= Hero.Health ? Hero.Health : EnergiRainObj.NumberOfEnergi / 2;
            int maxNumEnergi = EnergiRainObj.NumberOfEnergi >= Hero.Health ? Hero.Health : EnergiRainObj.NumberOfEnergi;
            var numberOfEnergiToCascade = Core.Aggregate.Instance.RNG(minNumEnergi, maxNumEnergi);
            for (int i = 0; i < numberOfEnergiToCascade; i++)
            {
                listDynamics.Add(new DynamicItem(startX, startY, Core.Aggregate.Instance.GetItem("energi"), whenToBeCollectable));
            }
        }
        public void DamageHero(Creature assalent, Creature victim, string från = "")
        {
            if (victim != null && victim.IsAttackable)
            {
                // cascade energi
                EnergiRainObj.MakeItRain = true;
                // hur många som ska kaskada
                EnergiRainObj.NumberOfEnergi = assalent.DamageGiven;
                // vilken position de ska ut gå ifrån
                EnergiRainObj.StartPosX = victim.px;
                EnergiRainObj.StartPosY = victim.py;




                // Attack victim with damage
                victim.Health -= assalent.DamageGiven;

                // Knock victim back
                float tx = victim.px - assalent.px;
                float ty = victim.py - assalent.py;
                float d = (float)Math.Sqrt(tx * tx + ty * ty);
                if (d < 1) d = 1.0f;

                var DX = (tx / d);
                var DY = (ty / d) - 1f;
                var DIST = 0.3f;
                victim.KnockBack(DX, DY, DIST);


                if (victim != Hero)
                {
                    victim.OnInteract(Hero);
                }
                else
                {
                    victim.SolidVsDynamic = true;
                }



            }

        }
        public void JumpDamage(Creature assalent, Creature victim)
        {
            victim.Health = 0;
            victim.Redundant = true;
            victim.RemoveCount = 1;
        }
        public void ShowDialog(List<string> listLines)
        {
            ListDialogToShow = listLines;
            doShowDialog = true;
        }

        public void ChangeMap(string MapName, float x, float y)
        {
            if (MapName == "worldmap")
            {
                this.Machine.Switch(Enum.State.WorldMap);
            }
            else
            {
                ChangeMap(MapName, x, y, this.Hero);
            }


        }
        public void ChangeMap(string MapName, float x, float y, DynamicGameObject hero)
        {
            listDynamics.Clear();
            listDynamics.Add(hero);
            CurrentMap = Core.Aggregate.Instance.GetMap(MapName);

            hero.px = x;
            hero.py = y;

            CurrentMap.PopulateDynamics(listDynamics);


            foreach (var q in ListQuests)
            {
                q.PopulateDynamics(listDynamics, CurrentMap.Name);
            }
        }
        public void AddQuest(Quest quest)
        {
            //m_listQuests.Add(quest);
            // egentligen add to front
            var oldSwitchArou = new List<Quest>();
            oldSwitchArou.Add(quest);
            foreach (var q in ListQuests)
            {
                oldSwitchArou.Add(q);
            }

            ListQuests = oldSwitchArou;
        }
        public bool GiveItem(Item item)
        {
            ListItems.Add(item);
            return true;
        }
    }

}
