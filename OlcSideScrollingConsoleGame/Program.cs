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
        private int JumpButtonCounter { get; set; }

        const int ScreenW = 256;
        //const int ScreenH = 240;
        //const int ScreenW = 256;
        //const int ScreenH = 192;
        const int ScreenH = 224;
        const int PixW = 4;
        const int PixH = 4;
        const int FrameR = -1;

        private TimeSpan ActualTotalTime { get; set; }
        private TimeSpan GameTotalTime { get; set; }
        private TimeSpan EndTotalTime { get; set; }

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
                        Core.Aggregate.Instance.PutOnHighScore(new HighScoreObj { DateTime = DateTime.Now, Handle = highScoreName, TimeSpan = EndTotalTime });

                        Core.Aggregate.Instance.SaveHighScoreList();
                        //Core.Aggregate.Instance.ResetHighScore();


                        ButtonsHasGoneIdle = false;
                        this.Machine.Switch(Enum.State.HighScore);
                        HasSwitchedState = true;

                        //}
                        //


                        //justForNowHighScoreList.Add(new HighScoreObj { Handle = highScoreName });

                        //DrawBigText(highScoreName, 10, 180);
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
            DrawBigText("    Name  Time     Date", 8, 45);
            int idx = 0;
            foreach (var HighScoreRow in Core.Aggregate.Instance.GetHighScoreList())
            {
                idx++;
                int HSRY = idx * 10 + 50;
                var formatHandle = HighScoreRow.Handle;
                if (formatHandle.Length < 5)
                    formatHandle = HighScoreRow.Handle + "  ";

                DrawBigText(" " + idx + ". " + formatHandle + " " + HighScoreRow.TimeSpan.ToString("hh':'mm':'ss") + " " + HighScoreRow.DateTime.ToString("dd MMM yy"), 8, HSRY);

            }

            DrawBigText("Press any button", 8, 210);
        }

        bool returnToEndAfterHighScore = false;
        private void DisplayEnd(float elapsed)
        {
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
                    this.Machine.Switch(Enum.State.WorldMap);
                    HasSwitchedState = true;
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
            if (HasSwitchedState)
                HasSwitchedState = false;

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

            DrawBigText("Penguin After All", 4, 4);
            DrawBigText("Press any button", 8, 160);

        }


        //public bool MenuStartHasBeenReleased { get; set; }
        int selectedMenuItem = 1;


        public Enum.MenuState MenuState { get; set; } = Enum.MenuState.StartMenu;
        private void DisplayMenu(float elapsed)
        {
            if (HasSwitchedState)
                HasSwitchedState = false;

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
                            // DrawBigText(menuList[selectedMenuItem - 1], 45, 4);
                            this.Machine.Switch(Enum.State.HighScore);
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
            if (HasSwitchedState)
                HasSwitchedState = false;

            if (Core.Aggregate.Instance.Sound != null)
            {
                if (Core.Aggregate.Instance.Sound.isPlaying("puttekong.wav"))
                {
                    Core.Aggregate.Instance.Sound.stop("puttekong.wav");
                }

                if (!Core.Aggregate.Instance.Sound.isPlaying("uno.wav"))
                {
                    Core.Aggregate.Instance.Sound.play("uno.wav");
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
            }

            float corrWorldMapPosX = 2;
            float corrWorldMapPosY = 3;

            if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 1)
            {
                corrWorldMapPosX = 2;
                corrWorldMapPosY = 3;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 2)
            {
                corrWorldMapPosX = 5;
                corrWorldMapPosY = 3;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 3)
            {
                corrWorldMapPosX = 9;
                corrWorldMapPosY = 3;
            }
            else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 4)
            {
                corrWorldMapPosX = 11;
                corrWorldMapPosY = 5;
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
                    // TODO : hantera vilken värld man ska till
                    if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 1)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapone", 2, 3, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 2)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("maptwo", 2, 3, Hero);

                        this.Machine.Switch(Enum.State.GameMap);
                        HasSwitchedState = true;
                        ButtonsHasGoneIdle = false;

                        return;
                    }
                    else if (Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap == 3)
                    {

                        hasAccumulatedAllSpeed = false;
                        ChangeMap("mapthree", 2, 3, Hero);

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

                    if ((!no1 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 0) && (myObject.px >= 2 && myObject.px <= 2.05f))
                    {
                        /*Om vx är possitiv - instruktion att gå höger.
                         *Om vx är negativ - instruktion att gå vänster
                         *Om StageCompleted == 0 förbjud att gå till höger specifikt (i detta fall höger)*/
                        if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 0)
                        {
                            myObject.vx = 0;
                        }
                        no1 = true;
                        no2 = false;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 1;

                    }
                    else if ((!no2 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 1) && (myObject.px >= 5 && myObject.px <= 5.09f))
                    {
                        // förbjud att gå höger
                        if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 1)
                        {
                            myObject.vx = 0;
                        }
                        no1 = false;
                        no2 = true;
                        no3 = false;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 2;

                    }
                    else if ((!no3 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted == 2) && (myObject.px >= 9 && myObject.px <= 9.1f))
                    {
                        // förbjud att gå höger
                        if (myObject.vx > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 2)
                        {
                            myObject.vx = 0;
                        }
                        no1 = false;
                        no2 = false;
                        no3 = true;
                        no4 = false;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 3;

                    }
                    else if (!no4 && (myObject.px >= 11 && myObject.px <= 11.1f && myObject.py < 3.01))
                    {
                        // förbjud att gå ner
                        if (myObject.vy > 0 || Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted != 3)
                        {
                            myObject.vy = 0;
                        }
                        no1 = false;
                        no2 = false;
                        no3 = false;
                        no4 = true;
                        no5 = false;
                        no6 = false;
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 4;

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
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 4;
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
                        Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 0;
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
        //bool experiment = true;
        private void DisplayPause(float elapsed)
        {
            if (HasSwitchedState)
                HasSwitchedState = false;

            if (Core.Aggregate.Instance.Sound != null)
            {
                if (Core.Aggregate.Instance.Sound.isPlaying("uno.wav"))
                {
                    Core.Aggregate.Instance.Sound.pause("uno.wav");
                }

                if (Core.Aggregate.Instance.Sound.isPlaying("puttekong.wav"))
                {
                    Core.Aggregate.Instance.Sound.pause("puttekong.wav");
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
                                                    if ((IIP.Button0 && IIP.Button1) || Konami.AB)
                                                    {
                                                        if (!Konami.AB)
                                                        {
                                                            Konami.AB = true;
                                                            ButtonsHasGoneIdle = false;

                                                            this.Machine.Switch(Enum.State.WorldMap);
                                                            HasSwitchedState = true;
                                                            ActualTotalTime += new TimeSpan(7, 0, 0);

                                                            if (Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted < Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap)
                                                            {
                                                                Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap;
                                                            }

                                                            Konami.nope();
                                                            return;
                                                        }

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
        private void DisplayStage(float elapsed)
        {
            if (HasSwitchedState)
                HasSwitchedState = false;

            if (Core.Aggregate.Instance.Sound != null)
            {
                if (Core.Aggregate.Instance.Sound.isPlaying("uno.wav"))
                {
                    Core.Aggregate.Instance.Sound.stop("uno.wav");
                }

                if (!Core.Aggregate.Instance.Sound.isPlaying("puttekong.wav"))
                {
                    Core.Aggregate.Instance.Sound.play("puttekong.wav");
                }
            }


            Core.Aggregate.Instance.Script.ProcessCommands(elapsed);

            if (Hero.Health < 1)
            {
                this.Machine.Switch(Enum.State.GameOver);
                HasSwitchedState = true;
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
                    var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookUp = true;
                }
                else
                {
                    var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookUp = false;
                }

                //Down
                if (GetKey(Key.Down).Down || IIP.down)
                {
                    //Hero.vy = 6.0f;
                    var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookDown = true;
                }
                else
                {
                    var tempHeroObj = (DynamicCreatureHero)Hero;
                    tempHeroObj.LookDown = false;
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
                if (GetKey(Key.Space).Down || IIP.Button0)
                {
                    if (JumpButtonState < 3)
                        JumpButtonState++;

                    #region ogrinalhopp
                    //if (Hero.vy == 0)
                    //{
                    //    Core.Aggregate.Instance.Sound.play("Click.wav");
                    //    Hero.vy = -9.3f;
                    //}
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

                    if (HeroAirBornState == 0 && HeroLandedState > 0 && JumpButtonState == 1 && JumpButtonCounter == 0) // HeroAirBornState
                    {
                        Hero.vy -= 7.0f;
                        // JumpButtonDownRelease = false; //#1 för att "flyga"
                        JumpButtonCounter++;
                        Core.Aggregate.Instance.Sound.play("Click.wav");
                    }
                    //else if (HeroAirBornState > 0 && HeroLandedState == 0 && JumpButtonState == 1 && !JumpButtonDownRelease) // dubbel så länge hjälte är på väg upp
                    // else if (HeroLandedState == 0 && JumpButtonState == 1 && !JumpButtonDownRelease) // #2 för att "flyga"
                    else if (HeroLandedState == 0 && JumpButtonState == 1 && JumpButtonCounter == 1)
                    {
                        Core.Aggregate.Instance.Sound.play("Click.wav");
                        Hero.vy -= 5.0f; // Riktigt vajsing.. Ger olika höjd om man börjar testa på precis nivå
                        JumpButtonCounter++; // för att inte kunna flyga 
                    }
                    #endregion


                }
                else if (!GetKey(Key.Space).Pressed || !IIP.Button0)
                {
                    JumpButtonState = 0;
                    JumpButtonPressRelease = true;

                    if (HeroLandedState != 0)
                    {
                        JumpButtonDownRelease = true;
                        JumpButtonCounter = 0;
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
                        var turnPatrol = false;
                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(myObject.py + 0.9f)))
                        {
                            //NewObjectPosX = (int)NewObjectPosX + 1;
                            NewObjectPosX = NewObjectPosX + 0.1f;
                            myObject.vx = 0;

                            turnPatrol = true;
                        }

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
                    }
                    else // Moving Right
                    {
                        var turnPatrol = false;
                        if (CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + fBorder + 0.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + (1.0f - fBorder)), (int)(myObject.py + (1.0f - fBorder))))
                        {
                            NewObjectPosX = (int)NewObjectPosX;
                            myObject.vx = 0;

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

                    }

                    myObject.Grounded = false;


                    if (myObject.vy <= 0) // Moving Up
                    {

                        //Hjälten airborn
                        if (myObject.IsHero)
                        {

                            if (HeroAirBornState < 3)
                            {
                                HeroAirBornState++;
                            }
                        }

                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)NewObjectPosY) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)NewObjectPosY))
                        {
                            NewObjectPosY = (int)NewObjectPosY + 1;
                            myObject.vy = 0;
                        }

                        //Hjälten landat - reset 
                        if (myObject.IsHero)
                        {
                            HeroLandedState = 0;
                        }

                    }
                    else // Moving Down
                    {

                        if (CurrentMap.GetSolid((int)(NewObjectPosX + 0.0f), (int)(NewObjectPosY + 1.0f)) || CurrentMap.GetSolid((int)(NewObjectPosX + 0.9f), (int)(NewObjectPosY + 1.0f)))
                        {
                            NewObjectPosY = (int)NewObjectPosY;
                            myObject.vy = 0;
                            myObject.Grounded = true;


                            //Hjälten landat
                            if (myObject.IsHero)
                            {

                                if (HeroLandedState < 3)
                                {
                                    HeroLandedState++;
                                }


                                if (HeroLandedState <= 1)
                                {
                                    // Core.Aggregate.Instance.Sound.play("Click.wav");
                                }
                            }


                        }

                        //Hjälten airborn - reset 
                        if (myObject.IsHero)
                        {
                            HeroAirBornState = 0;
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
            //string msg = "player - x: " + Hero.px + " y: " + Hero.py;
            //DisplayDialog(new List<string>() { msg }, 10, 10);

            DisplayDialog(new List<string>() { "Air: " + HeroAirBornState + " Land: " + HeroLandedState + " Jump: " + JumpButtonState }, 10, 10);

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

            // klicka ut ur animation
            if (GetKey(Key.Space).Pressed || IIP.Button0)
            {
                this.Machine.Switch(Enum.State.GameMap);
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
                DrawBigText("Player Dead", 4, 4);
                DrawBigText("Press button to continue", 8, 160);
            }

            AnimationCount--;
        }
        #endregion

        #region Reset / Load / Save
        private void Reset()
        {
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

        }
        private void Load(int slot)
        {
            if (slot == 3)
            {
                Core.Aggregate.Instance.Settings.ActivePlayer = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree;
            }
            else if (slot == 2)
            {
                Core.Aggregate.Instance.Settings.ActivePlayer = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo;
            }
            else
            {
                Core.Aggregate.Instance.Settings.ActivePlayer = Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne;
            }


            ActualTotalTime = Core.Aggregate.Instance.Settings.ActivePlayer.Time;
            Clock.HardReset();

            Hero.Health = Core.Aggregate.Instance.Settings.ActivePlayer.HeroEnergi;
        }
        private void Save(int slot)
        {
            if (slot == 3)
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree = Core.Aggregate.Instance.Settings.ActivePlayer;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.Time = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.DateTime = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotThree.IsUsed = true;
            }
            else if (slot == 2)
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo = Core.Aggregate.Instance.Settings.ActivePlayer;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.Time = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.DateTime = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotTwo.IsUsed = true;
            }
            else
            {
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne = Core.Aggregate.Instance.Settings.ActivePlayer;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.HeroEnergi = Hero.Health;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.Time = GameTotalTime;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.DateTime = DateTime.Now;
                Core.Aggregate.Instance.Settings.SaveSlotsObjs.SlotOne.IsUsed = true;

                // TODO: avgöra om jag ska spara hero x y för placering på load

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
            string Text = Hero.Health.ToString() + "% " + GameTotalTime.ToString();
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
            Pixel color = (Pixel)Pixel.Presets.Black;

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
                    DrawBigText("Press Start to resume.", 25, 35);
                    DrawBigText("Press Select to return to world map.", 25, 45);
                }
            }
            else
            {
                DrawBigText("X: " + Hero.px + " Y: " + Hero.py, 25, 25);
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
