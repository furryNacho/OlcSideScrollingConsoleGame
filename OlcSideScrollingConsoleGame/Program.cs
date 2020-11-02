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

        private bool HasSwitchedState { get; set; } = false;

        private bool RightToAccessPodium { get; set; } = true; // once per game. must reset on new game!

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
        public List<int> EnergiIdLista { get; set; } = new List<int>();
        private EnergiRainObject EnergiRainObj { get; set; } = new EnergiRainObject();
        /// <summary>
        /// 0 = Hjälten på väg ner. Mellan 1 till 3 så länge har hjälten varit i luften.
        /// </summary>
        private int HeroAirBornState { get; set; }
        /// <summary>
        /// 0 = Hjälten är inte på marken. Mellan 1 till 3 så länge har hjälten varit på marken. 
        /// </summary>
        private int HeroLandedState { get; set; }
        /// <summary>
        /// 0 = Spelare har släppt hoppknapp. Mellan 1 till 3 så länge har spelaren hållt nere hoppknappen
        /// </summary>
        private int JumpButtonState { get; set; }
        private bool JumpButtonPressRelease { get; set; }
        private bool JumpButtonDownRelease { get; set; }
        private bool JumpButtonDownReleaseOnce { get; set; }
        private int JumpButtonCounter { get; set; }

        const int ScreenW = 256;
        //const int ScreenH = 240;
        //const int ScreenW = 256;
        //const int ScreenH = 192;
        const int ScreenH = 224;
        const int PixW = 4;//4
        const int PixH = 4;//4
        const int FrameR = -1;

        private TimeSpan ActualTotalTime { get; set; }
        private TimeSpan GameTotalTime { get; set; }
        private TimeSpan EndTotalTime { get; set; }
        // private int EndTotalPercent { get; set; }

        private KonamiObj Konami { get; set; } = new KonamiObj();

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
            //TODO: ljud.. Ingenting fungerar som det ska med den här skiten
            //this.Enable(Game.Subsystem.Audio);
            /*
            this.Enable(Game.Subsystem.Audio);
            Resx.Load((Game)this);
            this.PlaySound(Resx.BgMusic);
            this.Volume = 0.5f;
            this.machine = new StateMachine<HorseRun.Scenes>();
            this.machine.Switch(HorseRun.Scenes.Menu);
            this.Reset();
             */

            //this.Enable(Game.Subsystem.Fullscreen); // riktigt med lagg

            this.Machine = new StateMachine<Enum.State>();
            //this.Machine.Switch(Enum.State.GameMap);
            HasSwitchedState = true;
            this.Machine.Switch(Enum.State.SplashScreen);


            Core.Aggregate.Instance.Load(this);

            // Om jag skulle vilja lägga till sparad tid
            //ActualTotalTime = new TimeSpan(0, 0, 7, 0, 0);
            ActualTotalTime = new TimeSpan();

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
                    this.DisplaySettings(elapsed);
                    break;
                case Enum.State.GameOver:
                    this.DisplayGameOver(elapsed);
                    break;
                case Enum.State.EnterHighScore:
                    this.DisplayEnterHighScore(elapsed);
                    break;
                case Enum.State.HighScore:
                    this.DisplayHighScore(elapsed);
                    break;
                case Enum.State.End:
                    this.DisplayEnd(elapsed);
                    break;
            }
        }

        int SettingsSelectIndex = 1;
        private void DisplaySettings(float elapsed)
        {
            if (HasSwitchedState)
            {
                SettingsSelectIndex = 1;
                HasSwitchedState = false;
            }

            this.Clear((Pixel)Pixel.Presets.Black);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            //What to draw
            string header = "";
            string bread = "";
            List<OptionsObj> options = new List<OptionsObj>();
            switch (MenuState)
            {
                case Enum.MenuState.Audio:
                    // Slå av på ljud. Ja nej
                    header = "Audio";
                    string soundIs = Core.Aggregate.Instance.Settings.AudioOn ? "on" : "off";
                    bread = "Sound is " + soundIs;

                    options = new List<OptionsObj>() {
                        new OptionsObj { Display = "Turn Sound On" },
                        new OptionsObj { Display = "Turn Sound Off"  },
                        new OptionsObj { Display = "Back", OptionIsBack = true } };

                    break;
                case Enum.MenuState.ClearHighScore:

                    header = "Clear High Score";

                    bread = "Clear The High Score List?";

                    options = new List<OptionsObj>() {
                        new OptionsObj { Display = "Yes" },
                        new OptionsObj { Display = "No" },
                        new OptionsObj { Display = "Back", OptionIsBack = true }
                    };


                    break;
                case Enum.MenuState.Save:

                    header = "Select Slot To Save Your Game";

                    bread = "Selected Slot ";
                    switch (SettingsSelectIndex)
                    {
                        case 1:
                            bread += " One";
                            break;
                        case 2:
                            bread += " Two";
                            break;
                        case 3:
                            bread += " Three";
                            break;
                        default:
                            bread = "";
                            break;
                    }

                    options = DefaultListSave();

                    break;
                case Enum.MenuState.Load:

                    header = "Select Game To Load";

                    bread = "Selected Slot ";
                    switch (SettingsSelectIndex)
                    {
                        case 1:
                            bread += " One";
                            break;
                        case 2:
                            bread += " Two";
                            break;
                        case 3:
                            bread += " Three";
                            break;
                        default:
                            bread = "";
                            break;
                    }

                    options = DefaultListSave();

                    break;
                case Enum.MenuState.ClearSavedGame:
                    //Lista sparade spel.
                    //Nolla vald sparat spel
                    header = "Clear Saved Game";

                    bread = "Clear Slot";
                    switch (SettingsSelectIndex)
                    {
                        case 1:
                            bread += " One";
                            break;
                        case 2:
                            bread += " Two";
                            break;
                        case 3:
                            bread += " Three";
                            break;
                        default:
                            bread = "";
                            break;
                    }


                    options = DefaultListSave();

                    //string displayForSlotOne = "Empty";
                    //var slotOne = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne;
                    //if (slotOne.IsUsed)
                    //{
                    //    displayForSlotOne = "Empty"; // TODO: skriv nåt vettigt vad det är för savestate
                    //}
                    //string displayForSlotTwo = "Empty";
                    //var slotTwo = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo;
                    //if (slotTwo.IsUsed)
                    //{
                    //    displayForSlotTwo = "Empty"; // TODO: skriv nåt vettigt vad det är för savestate
                    //}
                    //string displayForSlotThree = "Empty";
                    //var slotThree = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree;
                    //if (slotThree.IsUsed)
                    //{
                    //    displayForSlotThree = "Empty"; // TODO: skriv nåt vettigt vad det är för savestate
                    //}

                    //options = new List<OptionsObj>() {
                    //    new OptionsObj { Display = "1 "+ displayForSlotOne, OptionIsSlotOne = true },
                    //    new OptionsObj { Display = "2 "+ displayForSlotTwo, OptionIsSlotTwo = true },
                    //    new OptionsObj { Display = "3 "+ displayForSlotThree, OptionIsSlotThree = true},
                    //    new OptionsObj { Display = "Back", OptionIsBack = true },
                    //};

                    break;
                default:
                case Enum.MenuState.StartMenu:
                    break;
            }

            //Draw
            int HeaderX = (ScreenW / 2) - ((header.Length * 8) / 2);
            DrawBigText(header, HeaderX, 4);

            int breadX = (ScreenW / 2) - ((bread.Length * 8) / 2);
            DrawBigText(bread, breadX, 18);

            int idx = 0;
            foreach (var option in options)
            {
                idx++;
                string textRow = option.Display;
                if (SettingsSelectIndex == idx)
                {
                    textRow = "> " + option.Display + " <";
                }
                int optionX = (ScreenW / 2) - ((textRow.Length * 8) / 2);
                DrawBigText(textRow, optionX, 26 + idx * 12);
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
                if (ButtonsHasGoneIdle && (GetKey(Key.Up).Pressed || IIP.up))
                {

                    if (SettingsSelectIndex <= 1)
                    {
                        SettingsSelectIndex = options.Count;
                    }
                    else
                    {
                        SettingsSelectIndex--;
                    }

                    ButtonsHasGoneIdle = false;
                }
                //Down
                if (ButtonsHasGoneIdle && (GetKey(Key.Down).Pressed || IIP.down))
                {

                    if (SettingsSelectIndex >= options.Count)
                    {
                        SettingsSelectIndex = 1;
                    }
                    else
                    {
                        SettingsSelectIndex++;
                    }

                    ButtonsHasGoneIdle = false;
                }

                // Select
                if (ButtonsHasGoneIdle && (GetKey(Key.S).Pressed || IIP.Button7 || IIP.Button0))
                {
                    var SelectedOption = options[SettingsSelectIndex - 1];
                    ButtonsHasGoneIdle = false;

                    //Back to menu
                    if (SelectedOption.OptionIsBack)
                    {
                        ButtonsHasGoneIdle = false;
                        HasSwitchedState = true;

                        if (MenuState == Enum.MenuState.Load)
                            MenuState = Enum.MenuState.StartMenu;
                        else if (MenuState == Enum.MenuState.Save)
                            MenuState = Enum.MenuState.PauseMenu;
                        else
                            MenuState = Enum.MenuState.SettingsMenu;

                        this.Machine.Switch(Enum.State.Menu);
                    }
                    else if (MenuState == Enum.MenuState.Audio)
                    {
                        ButtonsHasGoneIdle = false;

                        if (SelectedOption.Display == "Turn Sound On")
                        {
                            Core.Aggregate.Instance.Settings.AudioOn = true;
                            Core.Aggregate.Instance.Sound.unMute();
                        }
                        else if (SelectedOption.Display == "Turn Sound Off")
                        {
                            Core.Aggregate.Instance.Settings.AudioOn = false;
                            Core.Aggregate.Instance.Sound.mute();
                        }

                        Core.Aggregate.Instance.SaveSettings();

                    }
                    else if (MenuState == Enum.MenuState.ClearHighScore)
                    {
                        if (SelectedOption.Display == "Yes")
                        {
                            Core.Aggregate.Instance.ResetHighScore();

                            MenuState = Enum.MenuState.SettingsMenu;
                            ButtonsHasGoneIdle = false;
                            HasSwitchedState = true;
                            this.Machine.Switch(Enum.State.Menu);

                        }
                        else if (SelectedOption.Display == "No")
                        {
                            MenuState = Enum.MenuState.SettingsMenu;
                            ButtonsHasGoneIdle = false;
                            HasSwitchedState = true;
                            this.Machine.Switch(Enum.State.Menu);
                        }

                    }
                    else if (MenuState == Enum.MenuState.ClearSavedGame)
                    {
                        ButtonsHasGoneIdle = false;

                        if (SelectedOption.OptionIsSlotOne)
                        {
                            //Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne = new SaveSlot() { Name = "Slot One" };
                            Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne = new SaveSlot();
                        }
                        else if (SelectedOption.OptionIsSlotTwo)
                        {
                            Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo = new SaveSlot();
                        }
                        else if (SelectedOption.OptionIsSlotThree)
                        {
                            Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree = new SaveSlot();
                        }

                        Core.Aggregate.Instance.SaveSettings();

                    }
                    else if (MenuState == Enum.MenuState.Load)
                    {
                        ButtonsHasGoneIdle = false;

                        if (SelectedOption.SlotIsUsed)
                        {
                            if (SelectedOption.OptionIsSlotOne)
                            {
                                Load(1);
                            }
                            else if (SelectedOption.OptionIsSlotTwo)
                            {
                                Load(2);
                            }
                            else if (SelectedOption.OptionIsSlotThree)
                            {
                                Load(3);
                            }

                            this.Machine.Switch(Enum.State.WorldMap);
                            HasSwitchedState = true;
                            ButtonsHasGoneIdle = false;
                        }

                    }
                    else if (MenuState == Enum.MenuState.Save)
                    {
                        //Spara spelet (indikera att spelet är sparat)
                        ButtonsHasGoneIdle = false;

                        if (SelectedOption.OptionIsSlotOne)
                        {
                            Save(1);
                        }
                        else if (SelectedOption.OptionIsSlotTwo)
                        {
                            Save(2);
                        }
                        else if (SelectedOption.OptionIsSlotThree)
                        {
                            Save(3);
                        }

                        Core.Aggregate.Instance.SaveSettings();

                    }
                }
            }

        }

        private List<OptionsObj> DefaultListSave()
        {
            string displayForSlotOne = "Empty";
            if (Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.IsUsed)
            {
                displayForSlotOne = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.DateTime.ToString("dd MMM yy") + " " + Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.StageCompleted.ToString();
            }
            string displayForSlotTwo = "Empty";
            if (Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.IsUsed)
            {
                displayForSlotTwo = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.DateTime.ToString("dd MMM yy") + " " + Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.StageCompleted.ToString();
            }
            string displayForSlotThree = "Empty";
            if (Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.IsUsed)
            {
                displayForSlotThree = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.DateTime.ToString("dd MMM yy") + " " + Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.StageCompleted.ToString();
            }

            return new List<OptionsObj>() {
                        new OptionsObj { Display = "1 "+ displayForSlotOne, OptionIsSlotOne = true,OptionIsSlotTwo = false, OptionIsSlotThree = false, SlotIsUsed = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.IsUsed},
                        new OptionsObj { Display = "2 "+ displayForSlotTwo, OptionIsSlotTwo = true, OptionIsSlotOne = false, OptionIsSlotThree = false, SlotIsUsed = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.IsUsed},
                        new OptionsObj { Display = "3 "+ displayForSlotThree, OptionIsSlotThree = true,OptionIsSlotOne = false,OptionIsSlotTwo = false, SlotIsUsed = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.IsUsed},
                        new OptionsObj { Display = "Back", OptionIsBack = true },
                    };
        }

        int HSSelectX = 0;
        int HSSelectY = 1;
        int Select = 1;
        List<int> NameInAscii = new List<int>() { 65, 65, 65 };
        //List<HighScoreObj> justForNowHighScoreList = new List<HighScoreObj>();
        private void DisplayEnterHighScore(float elapsed)
        {
            if (HasSwitchedState)
                HasSwitchedState = false;


            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);
            this.Clear((Pixel)Pixel.Presets.Black);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            List<HighScoreEnterName> ActionPlaceholder = new List<HighScoreEnterName>();


            #region temp fuckery
            HighScoreEnterName HSEN1 = new HighScoreEnterName()
            {
                MyProperty = "pilupp"
            };
            ActionPlaceholder.Add(HSEN1);
            HighScoreEnterName HSEN2 = new HighScoreEnterName()
            {
                MyProperty = "pilupp"
            };
            ActionPlaceholder.Add(HSEN2);
            HighScoreEnterName HSEN3 = new HighScoreEnterName()
            {
                MyProperty = "pilupp"
            };
            ActionPlaceholder.Add(HSEN3);
            HighScoreEnterName HSEN4 = new HighScoreEnterName()
            {
                MyProperty = "inget"
            };
            ActionPlaceholder.Add(HSEN4);
            HighScoreEnterName HSEN5 = new HighScoreEnterName()
            {
                MyProperty = "bokstav",
                letter = "A"
            };
            ActionPlaceholder.Add(HSEN5);
            HighScoreEnterName HSEN6 = new HighScoreEnterName()
            {
                MyProperty = "bokstav",
                letter = "A"
            };
            ActionPlaceholder.Add(HSEN6);
            HighScoreEnterName HSEN7 = new HighScoreEnterName()
            {
                MyProperty = "bokstav",
                letter = "A"
            };
            ActionPlaceholder.Add(HSEN7);
            HighScoreEnterName HSEN8 = new HighScoreEnterName()
            {
                MyProperty = "ok"
            };
            ActionPlaceholder.Add(HSEN8);
            HighScoreEnterName HSEN9 = new HighScoreEnterName()
            {
                MyProperty = "pilner"
            };
            ActionPlaceholder.Add(HSEN9);
            HighScoreEnterName HSEN10 = new HighScoreEnterName()
            {
                MyProperty = "pilner"
            };
            ActionPlaceholder.Add(HSEN10);
            HighScoreEnterName HSEN11 = new HighScoreEnterName()
            {
                MyProperty = "pilner"
            };
            ActionPlaceholder.Add(HSEN11);
            HighScoreEnterName HSEN12 = new HighScoreEnterName()
            {
                MyProperty = "inget"
            };
            ActionPlaceholder.Add(HSEN12);

            #endregion

            bool newTopScore = Core.Aggregate.Instance.IsNewFirstPlaceHS(EndTotalTime);

            //Draw
            if (newTopScore)
            {
                string gratz = "Congratulations!";
                int gratzX = (ScreenW / 2) - ((gratz.Length * 8) / 2);
                DrawBigText(gratz, gratzX, 8);
            }

            string Header = "New High Score";
            if (newTopScore)
            {
                Header = "You've Beaten The Top High Score";
            }
            int HeaderX = (ScreenW / 2) - ((Header.Length * 8) / 2);
            DrawBigText(Header, HeaderX, 20);

            string endTime = EndTotalTime.ToString("hh':'mm':'ss");
            int endTimeX = (ScreenW / 2) - ((endTime.Length * 8) / 2);
            DrawBigText(endTime, endTimeX, 35);

            string inst = "Enter Your Tag";
            int instX = (ScreenW / 2) - ((inst.Length * 8) / 2);
            DrawBigText(inst, instX, 58);


            #region Draw
            int i = 0;
            foreach (var item in ActionPlaceholder)
            {
                int x = i % 4;
                int y = i / 4;
                i++;


                //if (HSSelectX == x && HSSelectY == y)
                if (HSSelectX == x)
                {
                    Select = x;
                    //highlighted = item;
                }


                // Ska det vara Grå eller vit, upp eller ner, grå eller vit ok. Är det en bokstav, isf vilken bokstav? 

                //var fPoint = new Point(8 + x * 20, 20 + y * 20);
                var fPoint = new Point((8 + x * 20) + 90, (20 + y * 20) + 55);
                var sPoint = new Point(0, 0);

                if (item.MyProperty == "pilupp")
                {
                    if (Select + 1 == i)
                    {
                        // vit pil upp
                        sPoint = new Point(32, 48);
                        DrawPartialSprite(fPoint, SpriteItems, sPoint, 9, 4);
                    }
                    else
                    {
                        // grå (pil upp)
                        sPoint = new Point(32, 52);
                        DrawPartialSprite(fPoint, SpriteItems, sPoint, 9, 4);
                    }
                }
                else if (item.MyProperty == "inget")
                {
                    sPoint = new Point(16, 48);
                    DrawPartialSprite(fPoint, SpriteItems, sPoint, 9, 4);
                }
                else if (item.MyProperty == "ok")
                {
                    if (Select == 3)
                    {
                        //Om inte markerad
                        sPoint = new Point(48, 48);
                        DrawPartialSprite(fPoint, SpriteItems, sPoint, 16, 8);
                    }
                    else
                    {
                        // Om markerad
                        sPoint = new Point(48, 56);
                        DrawPartialSprite(fPoint, SpriteItems, sPoint, 16, 8);
                    }
                }
                else if (item.MyProperty == "pilner")
                {
                    if (Select + 9 == i)
                    {
                        // vit pil ner
                        sPoint = new Point(32, 60);
                        DrawPartialSprite(fPoint, SpriteItems, sPoint, 9, 4);
                    }
                    else
                    {
                        // grå pil ner
                        sPoint = new Point(32, 56);
                        DrawPartialSprite(fPoint, SpriteItems, sPoint, 9, 4);
                    }
                }
                else if (item.MyProperty == "bokstav")
                {
                    //DrawBigText(item.letter, fPoint.X, fPoint.Y);
                    var asciiChar = ((char)NameInAscii[x]).ToString();
                    DrawBigText(asciiChar, fPoint.X, fPoint.Y);
                }
                else
                {
                    DrawPartialSprite(fPoint, SpriteItems, sPoint, 16, 16);
                }


            }
            #endregion

            #region Selected And Input
            //// Draw selection reticule
            //var color = Pixel.FromRgb((uint)Pixel.Presets.White);
            //var pointParamOne = new Point(94 + (HSSelectX) * 20, (50 + 18 + (HSSelectY) * 20));
            //var pointParamTwo = new Point(94 + (HSSelectX + 1) * 20, (50 + 18 + (HSSelectY) * 20));
            //DrawLine(pointParamOne, pointParamTwo, color);
            //pointParamOne = new Point(94 + (HSSelectX) * 20, (50 + 18 + (HSSelectY + 1) * 20));
            //pointParamTwo = new Point(94 + (HSSelectX + 1) * 20, (50 + 18 + (HSSelectY + 1) * 20));
            //DrawLine(pointParamOne, pointParamTwo, color);
            //pointParamOne = new Point(94 + (HSSelectX) * 20, (50 + 18 + (HSSelectY) * 20));
            //pointParamTwo = new Point(94 + (HSSelectX) * 20, (50 + 18 + (HSSelectY + 1) * 20));
            //DrawLine(pointParamOne, pointParamTwo, color);
            //pointParamOne = new Point(94 + (HSSelectX + 1) * 20, (50 + 18 + (HSSelectY) * 20));
            //pointParamTwo = new Point(94 + (HSSelectX + 1) * 20, (50 + 18 + (HSSelectY + 1) * 20));
            //DrawLine(pointParamOne, pointParamTwo, color);




            //Input
            if (Focus)
            {
                //Button spam lock  
                if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    ButtonsHasGoneIdle = true;
                }

                //Selecting part
                if (ButtonsHasGoneIdle && (GetKey(Key.Left).Released || IIP.left))
                {
                    ButtonsHasGoneIdle = false;
                    HSSelectX--;
                }
                if (ButtonsHasGoneIdle && (GetKey(Key.Right).Released || IIP.right))
                {
                    ButtonsHasGoneIdle = false;
                    HSSelectX++;
                }

                if (ButtonsHasGoneIdle && (GetKey(Key.Up).Released || IIP.up))
                {
                    ButtonsHasGoneIdle = false;
                    if (Select < 3)
                    {
                        var newVal = NameInAscii[Select] + 1;
                        if (newVal > 126)
                            newVal = 32;
                        NameInAscii[Select] = newVal;
                    }
                }
                if (ButtonsHasGoneIdle && (GetKey(Key.Down).Released || IIP.down))
                {
                    ButtonsHasGoneIdle = false;
                    if (Select < 3)
                    {
                        var newVal = NameInAscii[Select] - 1;
                        if (newVal < 32)
                            newVal = 126;
                        NameInAscii[Select] = newVal;
                    }
                }

                if (HSSelectX < 0) HSSelectX = 3;
                if (HSSelectX >= 4) HSSelectX = 0;
                if (HSSelectY < 0) HSSelectY = 3;
                if (HSSelectY >= 4) HSSelectY = 0;


                //if (ButtonsHasGoneIdle && (GetKey(Key.Any).Pressed || !IIP.idle))
                if (ButtonsHasGoneIdle && (GetKey(Key.Escape).Pressed || IIP.Button7))
                {

                    ButtonsHasGoneIdle = false;
                    MenuState = Enum.MenuState.StartMenu;
                    this.Machine.Switch(Enum.State.Menu);
                    HasSwitchedState = true;
                    // return;
                }

                // OK
                if (ButtonsHasGoneIdle && (GetKey(Key.Space).Pressed || IIP.Button0))
                {
                    ButtonsHasGoneIdle = false;
                    if (Select == 3) //nollindex, select är på sista valet
                    {
                        var highScoreName = "";
                        foreach (var asciiLetter in NameInAscii)
                        {
                            var asciiChar = ((char)asciiLetter).ToString();
                            highScoreName += asciiChar;
                        }

                        //TODO: lägg till i highscorelistan

                        //Lite extra, just för att testa att lägga till
                        //if (Core.Aggregate.Instance.PlacesOnHighScore(EndTotalTime))
                        //{
                        //Core.Aggregate.Instance.PutOnHighScore(new HighScoreObj { DateTime = DateTime.Now, Handle = highScoreName, TimeSpan = EndTotalTime, Percent = Hero.Health }); TODO: EnergiIdLista
                        Core.Aggregate.Instance.PutOnHighScore(new HighScoreObj { DateTime = DateTime.Now, Handle = highScoreName, TimeSpan = EndTotalTime, Percent = EnergiIdLista.Count }); 

                        Core.Aggregate.Instance.SaveHighScoreList();
                        //Core.Aggregate.Instance.ResetHighScore();


                        ButtonsHasGoneIdle = false;
                        this.Machine.Switch(Enum.State.HighScore);
                        HasSwitchedState = true;

                        //}
                        //


                        //justForNowHighScoreList.Add(new HighScoreObj { Handle = highScoreName });

                        //DrawBigText(highScoreName, 10, 180);


                        //nollställ allt som är valt
                        NameInAscii = new List<int>() { 65, 65, 65 }; // sätt alla bokstäver till A
                        HSSelectX = 0; // sätt markören längst till vänster
                    }
                }

            }
            #endregion

            DrawBigText("Press OK when done", 8, 210);
        }

        private void DisplayHighScore(float elapsed)
        {
            if (HasSwitchedState)
                HasSwitchedState = false;

            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);
            this.Clear((Pixel)Pixel.Presets.Black);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            //GameTotalTime = Clock.Total + ActualTotalTime;//temp

            if (Focus)
            {
                //Button spam lock  
                if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
                {
                    ButtonsHasGoneIdle = true;
                }

                if (ButtonsHasGoneIdle && (GetKey(Key.Any).Pressed || !IIP.idle))
                //if (ButtonsHasGoneIdle && (GetKey(Key.P).Pressed || IIP.Button7))
                {
                    ButtonsHasGoneIdle = false;
                    HasSwitchedState = true;

                    if (returnToEndAfterHighScore)
                    {
                        returnToEndAfterHighScore = false;
                        
                        this.Machine.Switch(Enum.State.End);
                    }
                    else
                    {
                        MenuState = Enum.MenuState.StartMenu;
                        this.Machine.Switch(Enum.State.Menu);
                    }

                }

            }

            string Header = "Penguin After All High Score";
            int HeaderX = (ScreenW / 2) - ((Header.Length * 8) / 2);
            DrawBigText(Header, HeaderX, 10);
            //DrawBigText("    Name  Time     Date", 8, 45);
            DrawBigText("    Name  Time        %", 8, 45);
            int idx = 0;
            foreach (var HighScoreRow in Core.Aggregate.Instance.GetHighScoreList())
            {
                idx++;
                int HSRY = idx * 10 + 50;
                var formatHandle = HighScoreRow.Handle;
                if (formatHandle.Length < 5)
                    formatHandle = HighScoreRow.Handle + "  ";

                //DrawBigText(" " + idx + ". " + formatHandle + " " + HighScoreRow.TimeSpan.ToString("hh':'mm':'ss") + " " + HighScoreRow.DateTime.ToString("dd MMM yy"), 8, HSRY);
                DrawBigText(" " + idx + ". " + formatHandle + " " + HighScoreRow.TimeSpan.ToString("hh':'mm':'ss") + "    " + HighScoreRow.Percent, 8, HSRY);

            }

            DrawBigText("Press any button", 8, 210);
        }

        bool returnToEndAfterHighScore = false;
        private void DisplayEnd(float elapsed)
        {

            if (HasSwitchedState)
            {
                HasSwitchedState = false;
                if (Core.Aggregate.Instance.Sound != null)
                {

                    if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld))
                    {
                        Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld);
                    }

                    if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame))
                    {
                        Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame);
                    }

                    if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage))
                    {
                        Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage);
                    }

                    if (!Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundEnd))
                        Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundEnd);
                }
            }


            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);

            Core.Aggregate.Instance.Sound.stop();

            if (HasSwitchedState)
            {
                HasSwitchedState = false;

                if (RightToAccessPodium)
                {
                    EndTotalTime = GameTotalTime;
                    RightToAccessPodium = false;

                    if (Core.Aggregate.Instance.PlacesOnHighScore(EndTotalTime))
                    {
                        returnToEndAfterHighScore = true;

                        ButtonsHasGoneIdle = false;
                        this.Machine.Switch(Enum.State.EnterHighScore);
                        HasSwitchedState = true;

                        return;
                    }

                }
            }




            this.Clear((Pixel)Pixel.Presets.Black);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            Core.Aggregate.Instance.Settings.ActivePlayer.ShowEnd = false;

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
                    // TODO: om klar med spelet... Menu och eller reset. Kanske skulle ha reset som ett gamestate, där allt nollas och sätts igång


                    ButtonsHasGoneIdle = false;
                    //this.Machine.Switch(Enum.State.WorldMap);
                    this.Machine.Switch(Enum.State.Menu);
                    HasSwitchedState = true;
                    // return;
                }
            }


            //100%
            //TODO: alternativt slut.
            if (EnergiIdLista.Count == 100)
            {
                //100% av alla energier och 100% hälsa kvar
                if (Hero.Health == 100)
                {
                    //DrawSprite(new Point(0, 0), Core.Aggregate.Instance.GetSprite("splash"));
                    DrawSprite(new Point(0, 0), Core.Aggregate.Instance.GetSprite(Global.GlobalNamespace.SplashScreenRef.SuperAltEnd));
                    DrawBigText("End", 4, 4);
                    DrawBigText("100%", 4, 20);
                    DrawBigText("Press any button", 8, 160);
                }
                else
                {
                    //DrawSprite(new Point(0, 0), Core.Aggregate.Instance.GetSprite("splash"));
                    DrawSprite(new Point(0, 0), Core.Aggregate.Instance.GetSprite(Global.GlobalNamespace.SplashScreenRef.AltEnd));
                    DrawBigText("End", 4, 4);
                    DrawBigText("Exemplary display of skills", 4, 20);
                    DrawBigText("Press any button", 8, 160);
                }

            }
            else
            {
                //Klarade spelet
                //DrawSprite(new Point(0, 0), Core.Aggregate.Instance.GetSprite("splash"));
                DrawSprite(new Point(0, 0), Core.Aggregate.Instance.GetSprite(Global.GlobalNamespace.SplashScreenRef.End));
                DrawBigText("End", 4, 4);
                DrawBigText("You are super player", 4, 20);
                DrawBigText("Thank you for playing game", 4, 36);
                DrawBigText("Press any button", 8, 160);

            }

            //DrawBigText("End", 4, 4);
            //DrawBigText("You are super player", 4, 20);
            //DrawBigText("Thank you for playing game", 4, 36);
            //DrawBigText("Press any button", 8, 160);
        }

        public int countDownSplach { get; set; } = 60;
        private void DisplaySplashScreen(float elapsed)
        {
            if (HasSwitchedState)
                HasSwitchedState = false;

            if(countDownSplach > 0)
                countDownSplach--;

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            //Input
            if (Focus)
            {
                if (GetKey(Key.Any).Pressed || !IIP.idle)
                {
                    ButtonsHasGoneIdle = false;
                    MenuState = Enum.MenuState.StartMenu;
                    this.Machine.Switch(Enum.State.Menu);
                    HasSwitchedState = true;
                }
            }


            // Rita bakgrundsbild. 
            DrawSprite(new Point(0, 0), Core.Aggregate.Instance.GetSprite(Global.GlobalNamespace.SplashScreenRef.Start));



            //DrawBigText("Penguin After All", 4, 4);

            if (countDownSplach <= 0)
                DrawBigText("Press any button", 16, 180);

        }


        //public bool MenuStartHasBeenReleased { get; set; }
        int selectedMenuItem = 1;


        public Enum.MenuState MenuState { get; set; } = Enum.MenuState.StartMenu;
        private void DisplayMenu(float elapsed)
        {
            if (HasSwitchedState)
            {
                HasSwitchedState = false;
                return;
            }

            Core.Aggregate.Instance.Sound.pause();

            this.Clear((Pixel)Pixel.Presets.Black);

            string Header = "Menu";


            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;


            var menuList = new List<string>();


      
            //Pause on world map or start menu
            if (MenuState == Enum.MenuState.StartMenu)
            {
                menuList = new List<string>()
                {
                    "Start New Game",
                    "Load Saved Game",
                    "View High Score",
                    "Settings",
                    "Credits",
                    "Exit Game"
                };
            }
            else if (MenuState == Enum.MenuState.PauseMenu)
            {
                menuList = new List<string>()
                {
                    "Resume",
                    "Save",
                    "Quit"
                 };
            }
            else if (MenuState == Enum.MenuState.SettingsMenu)
            {
                Header = "Menu - Settings";

                menuList = new List<string>()
                {
                    "Audio",
                    "Clear High Score",
                    "Clear Saved Game",
                    "Back"
                 };
            }
            else if (MenuState == Enum.MenuState.CreditsMenu)
            {
                Header = "Credits";

                menuList = new List<string>()
                {
                    "Developer:",
                    "FurryNacho",
                    "olcPixelGameEngine:",
                    "Javidx9",
                    "Game Engine Port:",
                    "DevChrome",
                    "Music and Sound:",
                    "Fisk-i-fickorna",
                    "",
                    "Back"
                 };
                
                selectedMenuItem = menuList.Count;
            }
            //else if (MenuState == Enum.MenuState.Audio)
            //{
            //    menuList = new List<string>()
            //    {
            //        "Audio",
            //        "Clear High Score",
            //        "Clear Saved Game",
            //        "Back"
            //     };
            //}

            // Draw
            int HeaderX = (ScreenW / 2) - ((Header.Length * 8) / 2);
            DrawBigText(Header, HeaderX, 4);

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
                    if (MenuState != Enum.MenuState.CreditsMenu)
                    {
                        selectedMenuItem--;
                        ButtonsHasGoneIdle = false;
                    }
                }

                //Down
                if (selectedMenuItem < menuList.Count && (GetKey(Key.Down).Pressed || IIP.down) && ButtonsHasGoneIdle)
                {
                    selectedMenuItem++;
                    ButtonsHasGoneIdle = false;
                }

                // Select
                if (ButtonsHasGoneIdle && (GetKey(Key.S).Pressed || IIP.Button7 || IIP.Button0))
                {
                    ButtonsHasGoneIdle = false;

                    string selectedMenuItemString = menuList[selectedMenuItem - 1];
                    switch (selectedMenuItemString)
                    {

                        case "Start New Game":
                            selectedMenuItem = 1;

                            // TODO: ska faktiskt starta ett nytt spel.. inte bara börja där man slutade
                            Reset();
                            // TODO: spara settings lite snyggare
                            //Core.Aggregate.Instance.Settings.GameHasStarted = true;
                            //Core.Aggregate.Instance.SaveSettings(tempSaveSettings);

                            HasSwitchedState = true;
                            ButtonsHasGoneIdle = false;

                            this.Machine.Switch(Enum.State.WorldMap);

                            break;
                        case "Resume":
                            selectedMenuItem = 1;
                            this.Machine.Switch(Enum.State.WorldMap);
                            HasSwitchedState = true;
                            ButtonsHasGoneIdle = false;
                            break;
                        case "Save":
                            selectedMenuItem = 1;


                            ButtonsHasGoneIdle = false;
                            MenuState = Enum.MenuState.Save;
                            HasSwitchedState = true;
                            this.Machine.Switch(Enum.State.Settings);


                            break;
                        case "Load Saved Game":
                            selectedMenuItem = 1; // Kanske ska sätta till 2..

                            ButtonsHasGoneIdle = false;
                            MenuState = Enum.MenuState.Load;
                            HasSwitchedState = true;
                            this.Machine.Switch(Enum.State.Settings);

                            break;
                        case "Settings":
                            selectedMenuItem = 1;
                            //this.Machine.Switch(Enum.State.Settings);
                            //HasSwitchedState = true;
                            MenuState = Enum.MenuState.SettingsMenu;
                            ButtonsHasGoneIdle = false;
                            break;
                        case "Audio":
                            ButtonsHasGoneIdle = false;
                            MenuState = Enum.MenuState.Audio;
                            HasSwitchedState = true;
                            this.Machine.Switch(Enum.State.Settings);
                            break;
                        case "Clear High Score":
                            ButtonsHasGoneIdle = false;
                            MenuState = Enum.MenuState.ClearHighScore;
                            HasSwitchedState = true;
                            this.Machine.Switch(Enum.State.Settings);
                            break;
                        case "Clear Saved Game":
                            ButtonsHasGoneIdle = false;
                            MenuState = Enum.MenuState.ClearSavedGame;
                            HasSwitchedState = true;
                            this.Machine.Switch(Enum.State.Settings);
                            break;
                        case "Back":
                            selectedMenuItem = 1;
                            ButtonsHasGoneIdle = false;

                            //if (MenuState == Enum.MenuState.StartMenu)
                            //{
                            //}
                            //else
                            //{
                            //}
                            MenuState = Enum.MenuState.StartMenu;

                            break;
                        case "Quit":

                            // TODO: clean up.
                            //TODO: go back to main menu
                            //Core.Aggregate.Instance.GetSettings().GameHasStarted = false;
                            MenuState = Enum.MenuState.StartMenu;
                            ButtonsHasGoneIdle = false;

                            break;
                        case "Exit Game":
                            //Clean up sound
                            if (Core.Aggregate.Instance.Sound != null)
                                Core.Aggregate.Instance.Sound.cleanUp();

                            //Exit gameloop
                            Core.Aggregate.Instance.ThisGame.Finish();
                            break;
                        case "View High Score":
                            ButtonsHasGoneIdle = false;
                            this.Machine.Switch(Enum.State.HighScore);
                            HasSwitchedState = true;
                            break;
                        case "Credits":
                            selectedMenuItem = 3;
                            ButtonsHasGoneIdle = false;
                            MenuState = Enum.MenuState.CreditsMenu;
                            HasSwitchedState = true;
                            break;

                        //case "examp":
                        //    ButtonsHasGoneIdle = false;
                        //    MenuState = Enum.MenuState.StartMenu;

                        //    break;

                        default:
                            Core.Aggregate.Instance.ReadWrite.WriteToLog("DisplayMenu - Select value : " + selectedMenuItem + ". Default switch");
                            break;
                    }
                }

            }

           

        }


        bool unlockAllStages = false;
        bool no1 = true;
        bool no2 = false;
        bool no3 = false;
        bool no4 = false;
        bool no5 = false;
        bool no6 = false;
        bool no7 = false;
        bool no8 = false;
        int currentStage = 0;
        //int runStageNo = 1;
        bool hasAccumulatedAllSpeed = false;
        //public bool WorldmapStartHasBeenReleased { get; set; }
        private void DisplayWorldMap(float elapsed)
        {
            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);

            //if (HasSwitchedState)
            //{
            //    //HasSwitchedState = false;
            //    Hero.vx = 0;
            //    //return;
            //}

            if (currentStage == 0)
            {
                currentStage = Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted;
            }

            if (Core.Aggregate.Instance.Sound != null)
            {
                if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame))
                {
                    Core.Aggregate.Instance.Sound.stop(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame);
                }

                if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundEnd))
                {
                    Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundEnd);
                }

                if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage))
                {
                    Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage);
                }

                if (!Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld))
                {
                    Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld);
                }
            }

            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);


            if (Core.Aggregate.Instance.Settings.ActivePlayer.ShowEnd)
            {
                this.Machine.Switch(Enum.State.End);
                ButtonsHasGoneIdle = false;
                HasSwitchedState = true;
                return;
            }

            this.Clear((Pixel)Pixel.Presets.Black);

            if (!hasAccumulatedAllSpeed)
            {
                hasAccumulatedAllSpeed = true;
                Hero.vx = 0;
                Hero.vy = 0;
                Hero.ChangeStageKnockBackReset();
            }

            float corrWorldMapPosX = 3;
            float corrWorldMapPosY = 8;

            if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 1)
            {
                corrWorldMapPosX = 3;
                corrWorldMapPosY = 8;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 2)
            {
                corrWorldMapPosX = 6;
                corrWorldMapPosY = 8;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 3)
            {
                corrWorldMapPosX = 9;
                corrWorldMapPosY = 8;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 4)
            {
                corrWorldMapPosX = 12;
                corrWorldMapPosY = 8;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 5)
            {
                corrWorldMapPosX = 15;
                corrWorldMapPosY = 8;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 6)
            {
                corrWorldMapPosX = 18;
                corrWorldMapPosY = 8;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 7)
            {
                corrWorldMapPosX = 21;
                corrWorldMapPosY = 8;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 8)
            {
                corrWorldMapPosX = 24;
                corrWorldMapPosY = 8;
            }

            if (CurrentMap.Name != "worldmap")
            {
                //TODO: skicka in en placeholder och läsa settings för att avgöra vart på kartan ska placeras
                ChangeMap("worldmap", corrWorldMapPosX, corrWorldMapPosY, Hero);
                ButtonsHasGoneIdle = false;

            }

            if (HasSwitchedState)
            {
                Hero.vx = 0;
                Hero.vy = 0;
                HasSwitchedState = false;

                if (Hero.px != corrWorldMapPosX || Hero.py != corrWorldMapPosY)
                {
                    Hero.px = corrWorldMapPosX;
                    Hero.py = corrWorldMapPosY;
                }
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
                if (ButtonsHasGoneIdle && (GetKey(Key.Up).Down || IIP.up))
                {
                    Hero.vy = -3.0f;
                }

                //Down
                if (ButtonsHasGoneIdle && (GetKey(Key.Down).Down || IIP.down))
                {
                    Hero.vy = 3.0f;
                }

                //Right
                if (ButtonsHasGoneIdle && (GetKey(Key.Right).Down || IIP.right))
                {
                    //Förbjud gå höger
                    if (!unlockAllStages)
                    {
                        if (currentStage != 0 && currentStage <= Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted)
                        {
                            Hero.vx = 3;
                        }
                    }
                    else
                    {
                        //För test lås upp alla banor
                        Hero.vx = 3;
                    }
                }

                //Left
                if (ButtonsHasGoneIdle && (GetKey(Key.Left).Down || IIP.left))
                {

                    //Ska man någonsin få gå vänster? (blivit nåt knas om man gör höger får man ändå inte gå vänster igen. Om man inte hått höger får man gå vänster..?)
                    //Hero.vx = -3;
                }

                // A ("jump button")
                if (ButtonsHasGoneIdle && (GetKey(Key.Space).Pressed || IIP.Button0))
                {
                    // TODO : hantera vilken värld man ska till
                    if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 1)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapone", 2, 23, Hero); // Gör första banan till sista banan
                        //ChangeMap("mapnine", 1, 4, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 2)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("maptwo", 2, 23, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 3)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapthree", 2, 20, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 4)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapfour", 2, 3, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 5)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapfive", 2, 33, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 6)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapsix", 2, 22, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 7)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapseven", 3, 18, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 8)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapeight", 4, 41, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }

                    //return;
                }


                if (ButtonsHasGoneIdle && (GetKey(Key.P).Pressed || IIP.Button7))
                {
                    // Todo öppna meny
                    MenuState = Enum.MenuState.PauseMenu;
                    this.Machine.Switch(Enum.State.Menu);
                    HasSwitchedState = true;
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

                    //if ((!no1 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 0) && (myObject.px >= 3 && myObject.px <= 3.05f))
                    if (!no1 && (myObject.px >= 3 && myObject.px <= 3.05f))
                    {
                        /*Om vx är possitiv - instruktion att gå höger.
                         *Om vx är negativ - instruktion att gå vänster
                         *Om StageCompleted == 0 förbjud att gå till höger specifikt (i detta fall höger)*/
                        //if(!unlockAllStages)
                        //   if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 0)
                        //   {
                        //       myObject.vx = 0;
                        //   }

                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;


                        no1 = true;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        no7 = false;
                        no8 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 1;
                        currentStage = 1;

                    }
                    else if ((!no2 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 1) && (myObject.px >= 6 && myObject.px <= 6.09f))
                    {
                        // förbjud att gå höger
                        //if (!unlockAllStages)
                        //    if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 1)
                        //    {
                        //        myObject.vx = 0;
                        //    }

                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;

                        no1 = false;
                        no2 = true;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        no7 = false;
                        no8 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 2;
                        currentStage = 2;

                    }
                    else if ((!no3 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 2) && (myObject.px >= 9 && myObject.px <= 9.1f))
                    {
                        // förbjud att gå höger
                        //if (!unlockAllStages)
                        //    if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 2)
                        //{
                        //    myObject.vx = 0;
                        //}

                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;

                        no1 = false;
                        no2 = false;
                        no3 = true;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        no7 = false;
                        no8 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 3;
                        currentStage = 3;

                    }
                    //else if (!no4 && (myObject.px >= 11 && myObject.px <= 11.1f && myObject.py < 3.01))
                    else if ((!no4 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 3) && (myObject.px >= 12 && myObject.px <= 12.1f))
                    {
                        // förbjud att gå höger
                        //if (!unlockAllStages)
                        //    if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 3)
                        //    {
                        //        myObject.vx = 0;
                        //    }

                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;

                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = true;
                        no5 = false;
                        no6 = false;
                        no7 = false;
                        no8 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 4;
                        currentStage = 4;

                    }
                    //else if (!no5 && (myObject.px >= 11 && myObject.py >= 5f && myObject.py <= 5.1f))
                    else if ((!no5 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 4) && (myObject.px >= 15 && myObject.px <= 15.1f))
                    {

                        // förbjud att gå höger
                        //if (!unlockAllStages)
                        //    if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 4)
                        //    {
                        //        myObject.vx = 0;
                        //    }

                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;

                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = true;
                        no6 = false;
                        no7 = false;
                        no8 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 5;
                        currentStage = 5;
                    }
                    //else if (!no6 && (myObject.px >= 11 && myObject.py >= 7f && myObject.py <= 7.01f))
                    else if ((!no6 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 5) && (myObject.px >= 18 && myObject.px <= 18.1f))
                    {

                        // förbjud att gå höger
                        //if (!unlockAllStages)
                        //    if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 5)
                        //    {
                        //        myObject.vx = 0;
                        //    }


                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;

                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = true;
                        no7 = false;
                        no8 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 6;
                        currentStage = 6;
                    }
                    else if ((!no7 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 6) && (myObject.px >= 21 && myObject.px <= 21.1f))
                    {
                        // förbjud att gå höger
                        //if (!unlockAllStages)
                        //    if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 6)
                        //    {
                        //        myObject.vx = 0;
                        //    }


                        //myObject.vx = 0;
                        //myObject.vy = 0;
                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;

                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        no7 = true;
                        no8 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 7;
                        currentStage = 7;
                    }
                    else if ((!no8 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 7) && (myObject.px >= 24 && myObject.px <= 24.1f))
                    {
                        //myObject.vx = 0;
                        //myObject.vy = 0;

                        // förbjud att gå höger
                        //if (!unlockAllStages)
                        //    if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 7)
                        //    {
                        //        myObject.vx = 0;
                        //    }

                        if (myObject.vx > 0)
                        {
                        }
                        myObject.vx = 0;

                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        no7 = false;
                        no8 = true;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 8;
                        currentStage = 8;
                    }

                }

                //Sätta värder för att rätt overlay ska visas
                if (myObject.Name == "overlayworldmap")
                {
                    // StageCompleted ska vara blå. Alla under ska sättas till passed. Alla över not passed 
                    var CurrentStageThatIsCompleted = Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted;
                    if (myObject.Id < (CurrentStageThatIsCompleted+1))
                    {
                        myObject.StageStatus = Enum.StageStatus.Passed;
                    }
                    else if (myObject.Id == (CurrentStageThatIsCompleted+1))
                    {
                        myObject.StageStatus = Enum.StageStatus.Current;
                    }
                    else if (myObject.Id > (CurrentStageThatIsCompleted+1))
                    {
                        myObject.StageStatus = Enum.StageStatus.NotPassed;
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
                        NewObjectPosX = (int)NewObjectPosX;
                        myObject.vx = 0;
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

            //Om kamera inte ska följa spelare:
            //CameraPosX = 0;
            //CameraPosY = 0;
            //Låt kamera följa spelare:
            CameraPosX = Hero.px;
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
            //
            #endregion

            #region Draw obj's
            // Draw all objekts
            foreach (var myObject in listDynamics)
            {
                myObject.DrawSelf(this, fOffsetX, fOffsetY);
            }

            //Hack, rita alltid hjälten sist..
            var dynamicHeroObj = listDynamics.FirstOrDefault(x=>x.IsHero);
            if (dynamicHeroObj != null)
            {
                dynamicHeroObj.DrawSelf(this, fOffsetX, fOffsetY);
            }
            //

            #endregion
            //DrawBigText(wordMapText, 25, 25);
            var wordMapText = "        World Map. Stage: " + (currentStage == 0 ?  1 : currentStage)+"                     ";
            DrawBigText(wordMapText, 0, 217);
            DrawHUD();

            //string msg = "player - x: " + Hero.px + " y: " + Hero.py;
            //DisplayDialog(new List<string>() { msg }, 10, 10);
        }

        //public bool PauseStartHasBeenReleased { get; set; }
        //bool experiment = true;
        private void DisplayPause(float elapsed)
        {
            if (HasSwitchedState)
                HasSwitchedState = false;

            if (Core.Aggregate.Instance.Sound != null)
            {
                if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld))
                {
                    Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld);
                }

                if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame))
                {
                    Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame);
                }

                if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundEnd))
                {
                    Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundEnd);
                }

                if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage))
                {
                    Core.Aggregate.Instance.Sound.pause(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage);
                }

            }

            //Exempel på att nolla klockan 
            //if (experiment)
            //{
            //    experiment = false;
            //    ActualTotalTime = new TimeSpan();
            //    Clock.HardReset();
            //}


            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            DrawHUD("pause");

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
                    //if (Core.Aggregate.Instance.Settings.StageCompleted >= Core.Aggregate.Instance.Settings.SpawnAtWorldMap)
                    //{
                    ButtonsHasGoneIdle = false;
                    this.Machine.Switch(Enum.State.WorldMap);
                    HasSwitchedState = true;
                    //}
                }

                // Press start
                if (ButtonsHasGoneIdle && (GetKey(Key.P).Pressed || IIP.Button7))
                {
                    // GameStartHasBeenReleased = false;
                    ButtonsHasGoneIdle = false;
                    this.Machine.Switch(Enum.State.GameMap);
                    HasSwitchedState = true;
                }

                #region Konami
                if (ButtonsHasGoneIdle && !IIP.idle)
                {
                    if (IIP.up || Konami.up)
                    {
                        if (!Konami.up)
                        {
                            Konami.up = true;
                            ButtonsHasGoneIdle = false;
                            return;
                        }
                        if (IIP.up || Konami.upUp)
                        {
                            if (!Konami.upUp)
                            {
                                Konami.upUp = true;
                                ButtonsHasGoneIdle = false;
                                return;
                            }
                            if (IIP.down || Konami.down)
                            {
                                if (!Konami.down)
                                {
                                    Konami.down = true;
                                    ButtonsHasGoneIdle = false;
                                    return;
                                }
                                if (IIP.down || Konami.downDown)
                                {
                                    if (!Konami.downDown)
                                    {
                                        Konami.downDown = true;
                                        ButtonsHasGoneIdle = false;
                                        return;
                                    }
                                    if (IIP.left || Konami.left)
                                    {
                                        if (!Konami.left)
                                        {
                                            Konami.left = true;
                                            ButtonsHasGoneIdle = false;
                                            return;
                                        }
                                        if (IIP.right || Konami.right)
                                        {
                                            if (!Konami.right)
                                            {
                                                Konami.right = true;
                                                ButtonsHasGoneIdle = false;
                                                return;
                                            }
                                            if (IIP.left || Konami.leftLeft)
                                            {
                                                if (!Konami.leftLeft)
                                                {
                                                    Konami.leftLeft = true;
                                                    ButtonsHasGoneIdle = false;
                                                    return;
                                                }
                                                if (IIP.right || Konami.rightRight)
                                                {
                                                    if (!Konami.rightRight)
                                                    {
                                                        Konami.rightRight = true;
                                                        ButtonsHasGoneIdle = false;
                                                        return;
                                                    }
                                                    if (IIP.Button1 || Konami.B)
                                                    {
                                                        // TODO a sen b, inte ab

                                                        if (!Konami.B)
                                                        {
                                                            Konami.B = true;
                                                            ButtonsHasGoneIdle = false;
                                                            return;
                                                        }
                                                        if (IIP.Button0 || Konami.A)
                                                        {
                                                            Konami.A = true;
                                                            ButtonsHasGoneIdle = false;

                                                            this.Machine.Switch(Enum.State.WorldMap);
                                                            HasSwitchedState = true;
                                                            ActualTotalTime += new TimeSpan(1, 0, 0);

                                                            //Låsa upp alla banor:
                                                            unlockAllStages = true;

                                                            //"Klara av" den banan man är på:
                                                            //if (Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted < Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap)
                                                            //{
                                                            //    Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap;
                                                            //}
                                                            


                                                            Konami.nope();
                                                            return;
                                                        }
                                                        else
                                                            Konami.nope();
                                                    }
                                                    else
                                                        Konami.nope();
                                                }
                                                else
                                                    Konami.nope();
                                            }
                                            else
                                                Konami.nope();
                                        }
                                        else
                                            Konami.nope();
                                    }
                                    else
                                        Konami.nope();
                                }
                                else
                                    Konami.nope();
                            }
                            else
                                Konami.nope();
                        }
                        else
                            Konami.nope();
                    }
                    else
                        Konami.nope();
                }
                #endregion

            }
        }

        #region DisplayGameMap
        // public bool GameStartHasBeenReleased { get; set; }
        //private bool solidLeft { get; set; }

        public bool detHarBallatUrLog { get; set; }
        public float maxR { get; set; }
        public float maxL { get; set; }
        public float maxY { get; set; }
        public bool BPower { get; set; }
        public int rememberJumpCollision { get; set; }
        public int jumpMemory { get; set; }
        public int fallCounter { get; set; }
        public bool allowCoyoteTime { get; set; }
        // public int coyoteTime { get; set; }
        public int tempMemoryJumpCounter { get; set; }
        public int tempMemoryCayotyCounter { get; set; }
        public int enemyJump { get; set; }

        //public float fakk { get; set; }
        //public float fakkx { get; set; }


        private void DisplayStage(float elapsed)
        {
            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);

            Audio.Library.Sound playSounds = null;

            if (HasSwitchedState)
            {
                HasSwitchedState = false;

                // gå igenom listDynamics kolla picups. finns id i EnergiIdLista ta bort från listDynamics
                listDynamics.RemoveAll(x => EnergiIdLista.Any(y => y == x.CoinId));

                playSounds = Core.Aggregate.Instance.Sound;
                if (playSounds != null)
                {
                    if (Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld))
                    {
                        Core.Aggregate.Instance.Sound.stop(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundWorld);
                    }


                    //todo: kolla om sista bannan. spela sista låten
                    if (CurrentMap.Name != "mapnine")
                    {
                        if (!Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame))
                        {
                            Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundGame);
                        }
                    }
                    else
                    {
                        if (!Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage))
                        {
                            Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.BGSoundFinalStage);
                        }
                    }
                }

                //inget hopp när enter banan första gången
                Hero.vx = 0;
            }

            if (enemyJump > -1)
            {
                enemyJump--;
            }

            if (Hero.Health < 1)
            {
                this.Machine.Switch(Enum.State.GameOver);
                HasSwitchedState = true;
                ButtonsHasGoneIdle = false;
            }

            //listDynamics = listDynamics.Where(x => x.Redundant == false).ToList(); // Kanske ska dumpa denna för att kunna göra en poff på fiende om jump damage
            listDynamics = listDynamics.Where(x => x.RemoveCount < 4).ToList();

            if (listDynamics == null || listDynamics.Count <= 0)
            {
                throw new Exception();
            }


            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            detHarBallatUrLog = false;

            // Handle input
            if (Focus)
            {
                //B
                if (IIP.Button1 || IIP.Button2 || GetKey(Key.B).Down)
                {
                    BPower = true;
                }
                else if (!IIP.Button1)
                {
                    BPower = false;
                }

                var tempHeroObj = (DynamicCreatureHero)Hero;

                //Up
                if (GetKey(Key.Up).Down || IIP.up)
                {
                    //Hero.vy = -6.0f;
                    //var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookUp = true;
                }
                else
                {
                    //var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookUp = false;
                }

                //Down
                if (GetKey(Key.Down).Down || IIP.down)
                {
                    //Hero.vy = 6.0f;
                    //var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookDown = true;
                }
                else
                {
                    //var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookDown = false;
                }

                //Jump 
                if (GetKey(Key.Space).Down || IIP.Button0 || IIP.Button3)
                {
                    if (JumpButtonDownReleaseOnce) // hoppknapp måste ha släppts, hjälten måste vara airborn
                        jumpMemory = 5;

                    if (JumpButtonState < 3)
                        JumpButtonState++;

                    //coyoteTime allowCoyoteTime

                    #region ogrinalhopp
                    if ((Hero.vy == 0 && JumpButtonDownRelease) || (allowCoyoteTime && JumpButtonDownReleaseOnce) || enemyJump > -1)
                    {

                        if (Hero.vy != 0 && allowCoyoteTime)
                            tempMemoryCayotyCounter++;

                        if (Core.Aggregate.Instance.Sound != null)
                            Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.Jump); // TODO: hoppljud

                        Hero.vy = -9.3f;
                        JumpButtonDownRelease = false;
                        jumpMemory = -1;
                        enemyJump = -1;
                    }
                    #endregion

                    #region dutthopp
                    //if (HeroAirBornState == 0 && HeroLandedState > 0 && JumpButtonState == 1) // HeroAirBornState
                    //{
                    //    Hero.vy -= 4.65f;
                    //}
                    //else if (HeroAirBornState == 3 && HeroLandedState == 0 && JumpButtonState > 1 && JumpButtonDownRelease)
                    //{

                    //    Hero.vy -= 4.65f;
                    //    JumpButtonDownRelease = false;
                    //}
                    #endregion

                    #region dubbelhopp

                    //if (HeroAirBornState == 0 && HeroLandedState > 0 && JumpButtonState == 1 && JumpButtonCounter == 0) // HeroAirBornState
                    //{
                    //    Hero.vy -= 7.0f;
                    //    // JumpButtonDownRelease = false; //#1 för att "flyga"
                    //    JumpButtonCounter++;
                    //    Core.Aggregate.Instance.Sound.play("Click.wav");
                    //}
                    ////else if (HeroAirBornState > 0 && HeroLandedState == 0 && JumpButtonState == 1 && !JumpButtonDownRelease) // dubbel så länge hjälte är på väg upp
                    //// else if (HeroLandedState == 0 && JumpButtonState == 1 && !JumpButtonDownRelease) // #2 för att "flyga"
                    //else if (HeroLandedState == 0 && JumpButtonState == 1 && JumpButtonCounter == 1)
                    //{
                    //    Core.Aggregate.Instance.Sound.play("Click.wav");
                    //    Hero.vy -= 5.0f; // Riktigt vajsing.. Ger olika höjd om man börjar testa på precis nivå
                    //    JumpButtonCounter++; // för att inte kunna flyga 
                    //}
                    #endregion

                    JumpButtonDownReleaseOnce = false;
                }
                else if (!GetKey(Key.Space).Pressed || !IIP.Button0)
                {
                    JumpButtonDownReleaseOnce = true;

                    JumpButtonState = 0;
                    JumpButtonPressRelease = true;

                    if (HeroLandedState != 0)
                    {
                        JumpButtonDownRelease = true;
                        JumpButtonCounter = 0;
                    }
                }

                if (jumpMemory > 0 && Hero.Grounded)// && kanske Hero.vy == 0
                {
                    tempMemoryJumpCounter++;
     //               Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.Jump); 

                    Hero.vy = -9.3f;
                    JumpButtonDownRelease = false;
                    jumpMemory = -1;
                }

                //Right
                if (GetKey(Key.Right).Down || IIP.right)
                {
                    var newSpeed = (Hero.Grounded ? 25.0f : 15.0f) * elapsed;
                    Hero.vx += newSpeed;
                    if (BPower)
                    {
                        if (Hero.vx > 10)
                        {
                            Hero.vx = 10;
                        }
                    }
                    else
                    {
                        if (Hero.vx > 6)
                        {
                            Hero.vx = 6;
                        }
                    }

                    //solidLeft = false;
                }

                //Left
                if (GetKey(Key.Left).Down || IIP.left)
                {
                    var newSpeed = (Hero.Grounded ? -25.0f : -15.0f) * elapsed;
                    Hero.vx += newSpeed;
                    if (BPower)
                    {
                        if (Hero.vx < -10)
                        {
                            Hero.vx = -10;
                        }
                    }
                    else
                    {
                        if (Hero.vx < -6)
                        {
                            Hero.vx = -6;
                        }
                    }
                    //if (!solidLeft)
                    //{
                    //}
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
                    HasSwitchedState = true;
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

            //fakk

            //var fakkObjList = listDynamics.FirstOrDefault(x => x.Id == 1);
            //if (fakkObjList != null)
            //{
            //    fakk = fakkObjList.px;
            //    fakkx = fakkObjList.py;
            //}
            //


            foreach (var myObject in listDynamics)
            {
                myObject.detHarBallatUr = false;

                //Testar att flytta teleport med villkor
                if (CurrentMap.Name == "mapnine")
                {

                    if (!listDynamics.Any(x => x.Name == "boss"))
                    {

                        if (myObject is Teleport)
                        {
                            myObject.px = Hero.px;
                            myObject.py = Hero.py;
                        }
                    }
                    else 
                    {

                        if (myObject.Name == "boss")
                        {
                            Core.Aggregate.Instance.CheckSwitchX();
                        }

                        if (myObject.Id > 0)
                        {
                            myObject.px = Core.Aggregate.Instance.GetMyX(myObject.Id);
                        }

                       
                    }

                }
                //


                if (!myObject.Redundant)
                {




                    // Gravity
                    //myObject.vy += 20.0f * elapsed;
                    if (myObject.IsHero)
                    {
                        if (rememberJumpCollision > -1)
                        {
                            rememberJumpCollision--;
                        }

                        if (myObject.vy < 0)
                        {
                            if (BPower)
                            {
                                if (rememberJumpCollision < 0)
                                    myObject.vy += 17.0f * elapsed;
                            }
                            else
                            {
                                if (rememberJumpCollision < 0)
                                    myObject.vy += 20.0f * elapsed;
                            }
                        }
                        else
                        {
                            myObject.vy += 21.0f * elapsed;
                        }

                    }
                    else
                    {
                        myObject.vy += 20.0f * elapsed;
                    }


                    // Drag
                    if (myObject.IsHero && myObject.Grounded)
                    {
                        /* myObject.vx += -3.0f * myObject.vx * elapsed;
                            if (Math.Abs(myObject.vx) < 0.01f)
                                myObject.vx = 0.0f;
                         */

                        float slipperiness = 3.0f;
                        float slipperinessBPower = 0.09f;

                        //Make slippery if ice-lvl
                        if (CurrentMap.Name == "mapseven" || CurrentMap.Name == "mapeight" || CurrentMap.Name == "mapnine")
                        {
                            slipperiness = 0.1f;
                            slipperinessBPower = 0.08f;
                        }


                       
                        var anyDirectionAtAll = GetKey(Key.Left).Down || GetKey(Key.Right).Down || IIP.left || IIP.right;
                        if (!BPower)
                        {
                            myObject.vx += -3.0f * myObject.vx * elapsed;
                            if (!anyDirectionAtAll) 
                            {
                                if (Math.Abs(myObject.vx) < slipperiness)
                                    myObject.vx = 0.0f;
                            }

                        }
                        else if (BPower)
                        {
                            myObject.vx += -2.0f * myObject.vx * elapsed;
                            if (!anyDirectionAtAll) 
                            {
                                if (Math.Abs(myObject.vx) < slipperinessBPower)
                                    myObject.vx = 0.0f;
                            }

                        }
                    }

                    //temp högsta hast:
                    if (myObject.IsHero)
                    {
                        if (myObject.vx < maxL)
                        {
                            maxL = myObject.vx;
                        }
                        if (myObject.vx > maxR)
                        {
                            maxR = myObject.vx;
                        }
                    }
                    //end temp

                    // Clamp velocities
                    if (myObject.vx > 10.0f) // höger
                    {
                        if (myObject.vx > 11.0f)
                        {
                            myObject.detHarBallatUr = true;
                            detHarBallatUrLog = true;
                            myObject.vx = 0.0f;
                        }
                        else
                        {
                            myObject.vx = 10.0f;
                        }
                    }

                    if (myObject.vx < -10.0f) // vänster
                    {
                        if (myObject.vx < -11.0f)
                        {
                            myObject.detHarBallatUr = true;
                            detHarBallatUrLog = true;
                            myObject.vx = 0.0f;
                        }
                        else
                        {
                            myObject.vx = -10.0f;
                        }
                    }

                    if (myObject.vy > 20.0f)//neråt
                    {
                        if (myObject.vy > 25.0f)
                        {
                            myObject.detHarBallatUr = true;
                            detHarBallatUrLog = true;
                            myObject.vy = 0.0f;
                        }
                        else
                        {
                            myObject.vy = 20.0f;
                        }
                    }

                    if (myObject.vy < -10.0f)//uppåt
                    {
                        if (myObject.vy < -11.0f)
                        {
                            myObject.detHarBallatUr = true;
                            detHarBallatUrLog = true;
                            myObject.vy = 0.0f;
                        }
                        else
                        {
                            myObject.vy = -10.0f;
                        }
                    }
                    //End Clamp velocities


                    float NewObjectPosX = myObject.px + myObject.vx * elapsed;
                    float NewObjectPosY = myObject.py + myObject.vy * elapsed;



                    if (myObject.IsHero && rememberJumpCollision >= 0)
                    {
                        if (NewObjectPosY > myObject.py)
                        {
                            NewObjectPosY = myObject.py;
                        }
                    }

                    //
                    // Collision
                    //
                    float fBorder = 0.000000005f;// Hårdkoda hitbox (bevara för rpg!!)

                        // Moving Left
                    if (myObject.vx <= 0) 
                    {
                        var turnPatrol = false;
                        //if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.9f)))
                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0), (int)(myObject.py + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.9f)))
                        {
                            //if (myObject.IsHero)
                            //{
                            //  //  solidLeft = true;
                            //}
                            //NewObjectPosX = (int)NewObjectPosX + 1;
                            //NewObjectPosX = (int)NewObjectPosX + 0.9f;

                            if (myObject.Name != "frost")
                            {
                                myObject.vx = 0;
                            }
                            else
                            {

                            }

                            NewObjectPosX = (int)(NewObjectPosX + 0.9f);



                            turnPatrol = true;
                        }
                        //else
                        //{
                        //    if (myObject.IsHero)
                        //    {
                        //        solidLeft = false;
                        //    }
                        //}


                        if (myObject.Name == "walrus")
                        {
                            var x1 = (int)(NewObjectPosX + 0.0f);
                            var y1 = (int)(myObject.py + 0.0f) + 1; // +1 ner ett 
                            bool ena = CurrentMap.GetSolid(x1, y1);

                            var x2 = (int)(NewObjectPosX + 0.0f);
                            var y2 = (int)(myObject.py + 0.9f) + 1; // +1 ner ett 
                            bool andra = CurrentMap.GetSolid(x2, y2);



                            if (!ena || !andra || turnPatrol)
                            {
                                //NewObjectPosX = (int)NewObjectPosX + 1;
                                NewObjectPosX = NewObjectPosX + 0.1f;
                                myObject.vx = 2;
                                myObject.Patrol = Enum.Actions.Right;
                            }
                            else
                            {
                                myObject.Patrol = Enum.Actions.Left;
                            }


                        }


                        //frost hoppa vänster
                        if (myObject.Name == "frost")
                        {


                            var frostCreature = (Creature)myObject;

                            frostCreature.Ticker++;

                            if (frostCreature.DoSpecialAction)
                            {
                                var nextX = (int)(myObject.px + 0.0f);
                                var nextYPlusOne = (int)(myObject.py + 0.0f) + 1; // +1 ner ett 
                                bool objIsSolid = CurrentMap.GetSolid(nextX, nextYPlusOne);



                                if (objIsSolid && !frostCreature.HasJumped)
                                {
                                    myObject.vy = -7.7f;
                                    frostCreature.HasJumped = true;
                                }
                                else if (objIsSolid && frostCreature.HasJumped)
                                {

                                    if (frostCreature.DoneSpecialAction == 1)
                                    {
                                        frostCreature.Ticker = 0;
                                        frostCreature.DoSpecialAction = false;
                                        frostCreature.DoneSpecialAction = 0;
                                        frostCreature.HasJumped = false;
                                    }
                                    else
                                    {
                                        frostCreature.DoneSpecialAction = 1;
                                    }


                                }

                            }

                            if (!frostCreature.DoSpecialAction)
                            {


                                if (turnPatrol)
                                {
                                    NewObjectPosX = NewObjectPosX + 0.1f;
                                    myObject.vx = 1;
                                    myObject.Patrol = Enum.Actions.Right;



                                }
                                else
                                {
                                    myObject.Patrol = Enum.Actions.Left;

                                    //if (frostCreature.Ticker > 45)
                                    if (frostCreature.Ticker > 8)
                                    {
                                        frostCreature.DoSpecialAction = true;
                                    }

                                }



                            }

                            //
                            if ((int)(NewObjectPosX + 1.0f) <= frostCreature.FromCor)
                            {
                                NewObjectPosX = NewObjectPosX + 0.1f;
                                myObject.vx = 1;
                                myObject.Patrol = Enum.Actions.Right;
                            }
                            //

                        }





                    }
                    else // Moving Right
                    {


                        var turnPatrol = false;
                        if (CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + fBorder + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + (1.0f - fBorder))))
                        {
                            if (myObject.Name != "frost")
                            {

                                NewObjectPosX = (int)NewObjectPosX;

                                myObject.vx = 0;
                            }
                            else
                            {
                                // blir vacko om det är frost och frost hoppar. specialare för att det inte ska se ut som den har hicka
                                //NewObjectPosX = (int)NewObjectPosX+0.5f;
                                //myObject.vx = 4;
                                //myObject.vx = 8;


                            }


                            turnPatrol = true;
                        }

                        if (myObject.Name == "walrus")
                        {
                            var x1 = (int)(NewObjectPosX + (1.0f - fBorder));
                            var y1 = (int)(myObject.py + fBorder + 0.0f) + 1; // +1 ner ett 
                            bool ena = CurrentMap.GetSolid(x1, y1);

                            var x2 = (int)(NewObjectPosX + (1.0f - fBorder));
                            var y2 = (int)(myObject.py + (1.0f - fBorder) + 1); // +1 ner ett 
                            bool andra = CurrentMap.GetSolid(x2, y2);




                            if (!ena || !andra || turnPatrol)
                            {
                                NewObjectPosX = (int)NewObjectPosX;
                                myObject.vx = -2;
                                myObject.Patrol = Enum.Actions.Left;
                            }
                            else
                            {
                                myObject.Patrol = Enum.Actions.Right;
                            }

                        }


                        //frost hoppa höger
                        if (myObject.Name == "frost")
                        {


                            var frostCreature = (Creature)myObject;
                            frostCreature.Ticker++;



                            if (frostCreature.DoSpecialAction)
                            {
                                var nextX = (int)(myObject.px + 0.0f);
                                var nextYPlusOne = (int)(myObject.py + 0.0f) + 1; // +1 ner ett 
                                bool objIsSolid = CurrentMap.GetSolid(nextX, nextYPlusOne);



                                if (objIsSolid && !frostCreature.HasJumped)
                                {
                                    myObject.vy = -7.7f;
                                    frostCreature.HasJumped = true;
                                }
                                else if (objIsSolid && frostCreature.HasJumped)
                                {

                                    if (frostCreature.DoneSpecialAction == 1)
                                    {
                                        frostCreature.Ticker = 0;
                                        frostCreature.DoSpecialAction = false;
                                        frostCreature.DoneSpecialAction = 0;
                                        frostCreature.HasJumped = false;

                                    }
                                    else
                                    {
                                        frostCreature.DoneSpecialAction = 1;
                                    }


                                }

                            }

                            if (!frostCreature.DoSpecialAction)
                            {


                                if (turnPatrol)
                                {
                                    NewObjectPosX = (int)NewObjectPosX;
                                    myObject.vx = -1;
                                    myObject.Patrol = Enum.Actions.Left;




                                }
                                else
                                {
                                    myObject.Patrol = Enum.Actions.Right;

                                    //if (frostCreature.Ticker > 45)
                                    if (frostCreature.Ticker > 8)
                                    {
                                        frostCreature.DoSpecialAction = true;
                                    }

                                }



                            }

                            //
                            if ((int)(NewObjectPosX) >= frostCreature.ToCor)
                            {
                                NewObjectPosX = (int)NewObjectPosX;
                                myObject.vx = -1;
                                myObject.Patrol = Enum.Actions.Left;
                            }
                            //

                        }


                    }

                    myObject.Grounded = false;


                    // Moving Up
                    if (myObject.vy <= 0) 
                    {

                        //Hjälten airborn
                        if (myObject.IsHero)
                        {
                            jumpMemory = -1;

                            //coyoteTime = -1;

                            allowCoyoteTime = false;
                            fallCounter = 0;

                            if (HeroAirBornState < 3)
                            {
                                HeroAirBornState++;
                            }
                        }

                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)NewObjectPosY) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)NewObjectPosY))
                        {
                            NewObjectPosY = (int)NewObjectPosY + 1;
                            myObject.vy = 0;

                            //if (myObject.IsHero && IIP.Button1 && rememberJumpCollision < 0)
                            //{
                            //    rememberJumpCollision = 5;
                            //}
                            //if (myObject.IsHero && !IIP.Button1 && IIP.Button0 && rememberJumpCollision < 0)
                            //{
                            //    rememberJumpCollision = 5;
                            //}
                            if (myObject.IsHero && rememberJumpCollision < 0)
                            {
                                rememberJumpCollision = 5;
                            }
                        }

                        //Hjälten landat - reset 
                        if (myObject.IsHero)
                        {
                            HeroLandedState = 0;
                        }
                    }
                    else if (myObject.vy > 0)// Moving Down
                    {
                        //if (myObject.IsHero && myObject.vy > 1 && coyoteTime < 0)
                        //{
                        //    coyoteTime = 90;
                        //}
                        //if (coyoteTime >= 0)
                        //    coyoteTime--;

                        if(myObject.Name != "boss" && myObject.Name != "ice")
                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(NewObjectPosY + 1.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)(NewObjectPosY + 1.0f)))
                        {
                            NewObjectPosY = (int)NewObjectPosY;
                            myObject.vy = 0;
                            myObject.Grounded = true;


                            //Hjälten landat
                            if (myObject.IsHero)
                            {
                                fallCounter = 0;
                                allowCoyoteTime = true;

                                if (HeroLandedState < 3)
                                {
                                    HeroLandedState++;
                                }


                                if (HeroLandedState <= 1)
                                {
                                        if (Core.Aggregate.Instance.Sound != null)
                                            Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.Land); // spelaren landar
                                }
                            }


                        }

                        //Hjälten airborn - reset 
                        if (myObject.IsHero)
                        {
                            HeroAirBornState = 0;
                            if (jumpMemory >= 0)
                                jumpMemory -= 1;
                        }

                        if (myObject.IsHero && myObject.vy > 1 && fallCounter < 10)
                            fallCounter++;
                        if (fallCounter > 3) //3
                            allowCoyoteTime = false;
                    }



                    //
                    //
                    if (myObject.Name == "frost")
                    {

                        // om pos inte diffar mer än .3 per stickprov, troligen fast..

                        if (myObject.PrevTick == 10)
                        {
                            myObject.SampleOne = myObject.px;
                        }
                        else if (myObject.PrevTick == 30)
                        {
                            myObject.SampleTow = myObject.px;
                        }
                        else if (myObject.PrevTick == 50)
                        {
                            myObject.SampleThree = myObject.px;
                        }

                        if (myObject.PrevTick >= 50)
                        {

                            var checkOne = System.Math.Abs(myObject.SampleOne - myObject.SampleTow);
                            if (checkOne <= 0.3)
                            {
                                var checkTow = System.Math.Abs(myObject.SampleOne - myObject.SampleThree);
                                if (checkTow <= 0.3)
                                {
                                    var currP = myObject.px;
                                    var extraCheckOne = System.Math.Abs(currP - myObject.SampleOne);
                                    var extraCheckTow = System.Math.Abs(currP - myObject.SampleTow);
                                    var extraCheckThree = System.Math.Abs(currP - myObject.SampleThree);

                                    if (extraCheckOne < 5 && extraCheckTow < 5 && extraCheckThree < 5) // kolla änen nuvande mot prev
                                    {
                                        //myObject.py = myObject.py - 2;
                                        //NewObjectPosY = myObject.py;
                                        myObject.vy = -2;

                                        var logMove = true;
                                        if (logMove)
                                        {
                                            //TODO: logga att creature är flyttad
                                            Core.Aggregate.Instance.ReadWrite.WriteToLog($"Obj is stuck move");


                                        }
                                    }





                                }

                            }

                        }

                        if (myObject.PrevTick >= 60)
                        {
                            myObject.PrevTick = 0;
                        }

                        myObject.PrevTick++;
                    }
                    //
                    //


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
                                /* if (DynamicObjectPosX < (otherObject.px + 0.0f) &&
                                   (DynamicObjectPosX + 0.0f) > otherObject.px &&
                                    myObject.py < (otherObject.py + 0.0f) &&
                                   (myObject.py + 0.0f) > otherObject.py) */
                                if (DynamicObjectPosX < (otherObject.px + 1.0f) && (DynamicObjectPosX + 1.0f) > otherObject.px &&
                                     myObject.py < (otherObject.py + 1.0f) && (myObject.py + 1.0f) > otherObject.py)
                                {
                                    // First Check Horizontally 
                                    if (myObject.vx < 0)
                                    {
                                        // (krock från höger)
                                        DynamicObjectPosX = otherObject.px + 1.0f;

                                        //skada hero
                                        if (otherObject.Friendly != myObject.Friendly)
                                        {
                                            //if (otherObject.Friendly)
                                            if (otherObject.IsHero)
                                            {

                                                var victim = (Creature)otherObject;
                                                DamageHero((Creature)myObject, victim, "3");
                                            }
                                            else
                                            {
                                                //även om vänd bort från varandra
                                                DamageHero((Creature)otherObject, (Creature)myObject, "2");
                                            }
                                        }


                                    }
                                    else
                                    {
                                        // (krock från vänster)
                                        DynamicObjectPosX = otherObject.px - 1.0f;

                                        //  skada hero
                                        if (otherObject.Friendly != myObject.Friendly)
                                        {
                                            //if (otherObject.Friendly)
                                            if (otherObject.IsHero)
                                            {
                                                var victim = (Creature)otherObject;

                                                DamageHero((Creature)myObject, victim, "2");
                                            }
                                            else
                                            {
                                                //även om vänd bort från varandra
                                                DamageHero((Creature)otherObject, (Creature)myObject, "2");
                                            }
                                        }



                                    }

                                    //if (otherObject.Name == "walrus" || myObject.Name == "walrus")
                                    if ((myObject.Name == "walrus" || myObject.Name == "frost") && !otherObject.Friendly)
                                    {

                                        if (myObject.Patrol == Enum.Actions.Right)
                                        {
                                            myObject.Patrol = Enum.Actions.Left;
                                            myObject.vx = -2;
                                        }
                                        else
                                        {
                                            myObject.Patrol = Enum.Actions.Right;
                                            myObject.vx = 2;
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

                                        // (krock underifrån)
                                        DynamicObjectPosY = otherObject.py + 1.0f;

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
                                                    //Hero.vy = -8.5f;
                                                    Hero.vy = -5.5f;
                                                    Hero.Grounded = true;
                                                    JumpDamage((Creature)Hero, (Creature)myObject);
                                                }
                                            }
                                        }



                                    }
                                    else
                                    {
                                        //(krock från ovan)
                                        DynamicObjectPosY = otherObject.py - 1.0f; 


                                        // Kolla om krocken är mellan fiende och hjälte
                                        if (otherObject.Friendly != myObject.Friendly)
                                        {
                                            if (!otherObject.Friendly) // otherObject är fiende
                                            {

                                                if (otherObject.Name == "ice")
                                                {
                                                    DamageHero((Creature)otherObject, (Creature)Hero, "1");
                                                }
                                                else
                                                {

                                                    //studsa hjälten lite
                                                    Hero.vy = -5.5f;
                                                    Hero.Grounded = true;
                                                    JumpDamage((Creature)Hero, (Creature)otherObject);

                                                }
                                            }
                                            else
                                            {
                                                // fiende landar på hjälte?
                                                //var victim = (Creature)myObject;

                                                DamageHero((Creature)myObject, (Creature)Hero, "1");
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


                                        //
                                        //Plocka upp energi
                                        //
                                        if (playSounds != null)
                                        {
                                            //TODO: ljud plocka upp energi
                                            //if (!Core.Aggregate.Instance.Sound.isPlaying("puttekong.wav"))
                                            //{
                                            //    Core.Aggregate.Instance.Sound.play("puttekong.wav");
                                            //}

                                        }
                                        if (Core.Aggregate.Instance.Sound != null)
                                            if(Hero.IsAttackable)
                                                Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.PickUp);  // TODO: ljud för att plocka upp


                                        if (otherObject.CoinId > 0)
                                        {
                                            EnergiIdLista.Add(otherObject.CoinId);
                                        }


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
                    if (!myObject.detHarBallatUr)
                    {
                        myObject.px = DynamicObjectPosX;
                        myObject.py = DynamicObjectPosY;
                    }
                    else
                    {
                        if (detHarBallatUrLog && myObject.Name != "pickup")
                            Core.Aggregate.Instance.ReadWrite.WriteToLog($"Did not update position. Name: {myObject.Name}. Velocity X: {myObject.vx}. Velocuty Y: {myObject.vy}");
                    }
                    //End Apply new position

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
            //end link camera

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

            // vart är jag på kartan
            //string msg = "player - x: " + Hero.px + " y: " + Hero.py;splach
            //DisplayDialog(new List<string>() { msg }, 10, 10);
            // fakk
            ////string msgx = "player - x: " + fakk + " y: " + fakkx;
            ////DisplayDialog(new List<string>() { msgx }, 20, 20);


            //DisplayDialog(new List<string>() { "Air: " + HeroAirBornState + " Land: " + HeroLandedState + " Jump: " + JumpButtonState }, 10, 10);
            //DisplayDialog(new List<string>() { "vx: " + Hero.vx + " vy: " + Hero.vy }, 10, 10);




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
            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);

            if (HasSwitchedState)
                HasSwitchedState = false;



            this.Clear((Pixel)Pixel.Presets.Black);

            SlimDx.timer_Tick();
            IIP = SlimDx.IIP;

            // ge tillbaka lite liv
            if (Hero.Health < 1)
            {
                Hero.Health = 10;
            }

            //Button spam lock  
            if (!ButtonsHasGoneIdle && IIP.idle && !GetKey(Key.Any).Pressed)
            {
                ButtonsHasGoneIdle = true;
            }
            // klicka ut ur animation
            //if (ButtonsHasGoneIdle && (GetKey(Key.Space).Pressed || IIP.Button0))
            if (ButtonsHasGoneIdle && (GetKey(Key.Any).Pressed || !IIP.idle))
            {


                //this.Machine.Switch(Enum.State.GameMap);

                if (GetKey(Key.Space).Pressed || IIP.Button0)
                {
                    Reset();
                    //this.Machine.Switch(Enum.State.WorldMap);
                    ButtonsHasGoneIdle = false;
                    MenuState = Enum.MenuState.StartMenu;
                    this.Machine.Switch(Enum.State.Menu);
                }
                else
                {
                    ButtonsHasGoneIdle = false;
                    MenuState = Enum.MenuState.StartMenu;
                    this.Machine.Switch(Enum.State.Menu);
                }


                HasSwitchedState = true;
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
                DrawBigText("Player Dead", 8, 4);
                //DrawBigText("YOLO", 8, 8);
                //DrawBigText("Jump = restart. Any = Menu", 4, 32);
                //DrawBigText("Press button to continue", 8, 160);
                DrawBigText("Press any button", 8, 217);
            }

            AnimationCount--;
        }
        #endregion

        #region Reset / Load / Save
        private void Reset()
        {
            //TODO: reset enterhighscore

            //var fakeTeleport = listDynamics.FirstOrDefault(x => x.Name == "Teleport");
            //CurrentMap.OnInteraction(listDynamics, fakeTeleport, Enum.NATURE.WALK);

            // hjälten ska böra på 0
            // antal banor klarade 0
            // tid ska vara noll
            // liv ska vara 7  Hero.Health;
            // visa slut ska vara true
            Core.Aggregate.Instance.Settings.ActivePlayer = new SaveSlot();

            Hero.Health = Core.Aggregate.Instance.Settings.ActivePlayer.HeroEnergi;

            //Exempel på att nolla klockan 
            //if (experiment)
            //{
            //    experiment = false;
            //    ActualTotalTime = new TimeSpan();
            //    Clock.HardReset();
            //}
            // Om jag skulle vilja lägga till sparad tid
            //ActualTotalTime = new TimeSpan(0, 0, 7, 0, 0);
            ActualTotalTime = new TimeSpan();
            Clock.HardReset();
            RightToAccessPodium = true;
            //Core.Aggregate.Instance.Settings.ActivePlayer.ShowEnd = false;
            Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = 0;

            EnergiIdLista = new List<int>();

        }
        private void Load(int slot)
        {
            if (slot == 3)
            {
                //Core.Aggregate.Instance.Settings.ActivePlayer = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree;
                var copyObj = new SaveSlot()
                {
                    DateTime = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.DateTime,
                    Time = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.Time,
                    IsUsed = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.IsUsed,
                    HeroEnergi = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.HeroEnergi,
                    StageCompleted = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.StageCompleted,
                    SpawnAtWorldMap = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.SpawnAtWorldMap,
                    ShowEnd = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.ShowEnd,
                    EnergiCollected = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.EnergiCollected
                };
                ActualTotalTime = copyObj.Time;
                Core.Aggregate.Instance.Settings.ActivePlayer = copyObj;
                Hero.Health = copyObj.HeroEnergi;
                EnergiIdLista = copyObj.EnergiCollected;
            }
            else if (slot == 2)
            {
                //Core.Aggregate.Instance.Settings.ActivePlayer = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo;
                var copyObj = new SaveSlot()
                {
                    DateTime = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.DateTime,
                    Time = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.Time,
                    IsUsed = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.IsUsed,
                    HeroEnergi = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.HeroEnergi,
                    StageCompleted = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.StageCompleted,
                    SpawnAtWorldMap = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.SpawnAtWorldMap,
                    ShowEnd = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.ShowEnd,
                    EnergiCollected = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.EnergiCollected
                };
                ActualTotalTime = copyObj.Time;
                Core.Aggregate.Instance.Settings.ActivePlayer = copyObj;
                Hero.Health = copyObj.HeroEnergi;
                EnergiIdLista = copyObj.EnergiCollected;
            }
            else
            {
                //Core.Aggregate.Instance.Settings.ActivePlayer = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne;
                var copyObj = new SaveSlot()
                {
                    DateTime = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.DateTime,
                    Time = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.Time,
                    IsUsed = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.IsUsed,
                    HeroEnergi = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.HeroEnergi,
                    StageCompleted = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.StageCompleted,
                    SpawnAtWorldMap = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.SpawnAtWorldMap,
                    ShowEnd = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.ShowEnd,
                    EnergiCollected = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.EnergiCollected
                };
                ActualTotalTime = copyObj.Time;
                Core.Aggregate.Instance.Settings.ActivePlayer = copyObj;
                Hero.Health = copyObj.HeroEnergi;
                EnergiIdLista = copyObj.EnergiCollected;
            }


            //ActualTotalTime = Core.Aggregate.Instance.Settings.ActivePlayer.Time;
            Clock.HardReset();

            //Hero.Health = Core.Aggregate.Instance.Settings.ActivePlayer.HeroEnergi;
        }
        private void Save(int slot)
        {
            var copyObj = new SaveSlot()
            {
                DateTime = Core.Aggregate.Instance.Settings.ActivePlayer.DateTime,
                Time = Core.Aggregate.Instance.Settings.ActivePlayer.Time,
                IsUsed = Core.Aggregate.Instance.Settings.ActivePlayer.IsUsed,
                HeroEnergi = Core.Aggregate.Instance.Settings.ActivePlayer.HeroEnergi,
                StageCompleted = Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted,
                SpawnAtWorldMap = Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap,
                ShowEnd = Core.Aggregate.Instance.Settings.ActivePlayer.ShowEnd,
                EnergiCollected = EnergiIdLista
            };

            if (slot == 3)
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree = copyObj;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.Time = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.DateTime = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.IsUsed = true;


            }
            else if (slot == 2)
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo = copyObj;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.Time = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.DateTime = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.IsUsed = true;
            }
            else
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne = copyObj;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.Time = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.DateTime = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.IsUsed = true;
            }


        }
        #endregion

        void DrawHUD(string mode = "")
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
            //var fullTime = Clock.Total.ToString();

            GameTotalTime = Clock.Total + ActualTotalTime;

            //int idx = fullTime.LastIndexOf('.');
            //string Text = Hero.Health.ToString() + "% "+ fullTime.Substring(0, idx+2);
            //string Text = Hero.Health.ToString() + "% " + fullTime;
            string Text = Hero.Health.ToString() + "% " + GameTotalTime.ToString("hh':'mm':'ss'.'fff");
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
            //Pixel color = (Pixel)Pixel.Presets.DarkGreen;
            Pixel color = (Pixel)Pixel.Presets.Green;

            int EnergiOmeter = 14; // 14 är full. Max liv är 99. 
            //int RoundEnergiOmeter = 99 / Hero.Health;
            int RoundEnergiOmeter = Hero.Health;
            #region en uppsjö med om
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
                //color = (Pixel)Pixel.Presets.Green;
            }
            else if (RoundEnergiOmeter >= 28 && RoundEnergiOmeter < 35)
            {
                EnergiOmeter = 5;
                //color = (Pixel)Pixel.Presets.Green;
            }
            else if (RoundEnergiOmeter >= 35 && RoundEnergiOmeter < 42)
            {
                EnergiOmeter = 6;
                //color = (Pixel)Pixel.Presets.Green;
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
            #endregion
            var point5 = new Point(WorldX + 1, WorldY + 1); //start
            var point6 = new Point(WorldX + EnergiOmeter, WorldY + 1); //end
            DrawLine(point5, point6, color);

            var point7 = new Point(WorldX + 1, WorldY + 2); //start
            var point8 = new Point(WorldX + EnergiOmeter, WorldY + 2); //end
            DrawLine(point7, point8, color);

            if (!string.IsNullOrEmpty(mode))
            {
                if (mode == "pause")
                {
                    DrawBigText("Pause", 25, 25);
                    DrawBigText("Start resume.", 25, 35);
                    DrawBigText("Select go back.", 25, 45);
                }
            }
            else
            {
                //DrawBigText("X: " + Hero.px + " Y: " + Hero.py, 25, 25);
                //DrawBigText("vX: " + Hero.vx+" vY: "+Hero.vy, 25, 25);
                //DrawBigText("memoryjump: " + tempMemoryJumpCounter, 25, 30);
                //DrawBigText("CayotyCounter: " + tempMemoryCayotyCounter, 25, 40);
                //DrawBigText("allowCoyoteTime: " + allowCoyoteTime, 35, 40);


            }

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

            if (Core.Aggregate.Instance.Sound != null)
            {
                //mutePickup = true;

                Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.DamageHero);


                //if (!Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.Jump))
                //{
                //    Core.Aggregate.Instance.Sound.stop(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.Jump);
                //}
                //if (!Core.Aggregate.Instance.Sound.isPlaying(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.PickUp))
                //{
                //    Core.Aggregate.Instance.Sound.stop(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.PickUp);
                //}
            }



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
            if (Core.Aggregate.Instance.Sound != null)
                Core.Aggregate.Instance.Sound.play(OlcSideScrollingConsoleGame.Global.GlobalNamespace.SoundRef.Damage);

            if (victim.Name == "boss")
            {
                if (victim.IsAttackable)
                {
                    victim.IsAttackable = false;
                    victim.Health = victim.Health - 10;
                    if (victim.Health <= 0)
                    {
                        victim.Health = 0;
                        victim.Redundant = true;
                        victim.RemoveCount = 1;

                        enemyJump = 5;
                    }
                }
            }
            else if (victim.Name == "ice")
            {
                //indestructible?
            }
            else
            {
                victim.Health = 0;
                victim.Redundant = true;
                victim.RemoveCount = 1;

                enemyJump = 5;
            }

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
                HasSwitchedState = true;
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
