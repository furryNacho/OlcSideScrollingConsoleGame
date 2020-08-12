using PixelEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OlcSideScrollingConsoleGame.Models;
using OlcSideScrollingConsoleGame.Models.Items;
using OlcSideScrollingConsoleGame.Commands;
using Audio.Library;

namespace OlcSideScrollingConsoleGame.Core
{
    public class Aggregate
    {
        private static readonly object padlock = new object();
        private static Aggregate instance = null;

        Aggregate()
        {
        }
        public static Aggregate Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Aggregate();
                    }
                    return instance;
                }
            }
        }

        public Program ThisGame { get; set; }
        public ReadWrite ReadWrite { get; set; }
        private Dictionary<string, Sprite> MapSprites { get; set; } = new Dictionary<string, Sprite>();
        private Dictionary<string, Map> MapMaps { get; set; } = new Dictionary<string, Map>();
        private Dictionary<string, Item> MapItems { get; set; } = new Dictionary<string, Item>();
        private Dictionary<string, LevelObj> MapData { get; set; } = new Dictionary<string, LevelObj>();
        private string PathSprites { get { return @"\Resources\Assets\Sprites"; } }
        private string PathMapData { get { return @"\Resources\Assets\MapData"; } }
        private string PathSettings { get { return @"\Resources\Settings"; } }
        private string PathSound { get { return @"\Resources\Assets\Sound"; } }
        public ScriptProcessor Script { get; set; }
        private Random Random { get; set; } = new Random();

        public SettingsObj Settings { get; set; } = new SettingsObj();
        private List<HighScoreObj> HighScoreList { get; set; }

        public Audio.Library.Sound Sound { get; private set; }

        internal void Load(Program game)
        {
            ThisGame = game;
            ReadWrite = new ReadWrite();
            LoadSprites();
            LoadAllMapData();
            LoadMaps();
            LoadItems();
            Script = new ScriptProcessor();

            LoadSettings(); 
            LoadHighScore();

            LoadSound();
        }

        public void LoadSound()
        {
            try
            {
                Sound = Audio.Library.Sound.Instance;
                //var initSuccess = Sound.init();
                Sound.loadSound("uno.wav");
                Sound.loopSound("uno.wav");
                Sound.loadSound("Click.wav");
                Sound.loadSound("puttekong.wav");
                //Sound.loopSound("Click.wav");
                //Sound.play("Piano.wav");

                if (Settings != null)
                    if (Settings.AudioOn)
                    {
                       Sound.unMute();
                    }
                    else if(!Settings.AudioOn)
                    {
                        Sound.mute();
                    }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void LoadSprites()
        {
            LoadSprite("tilesheetspring", PathSprites, @"\tilesheetspring", ".png");
            LoadSprite("tilesheetsummer", PathSprites, @"\tilesheetsummer", ".png");
            LoadSprite("tilesheetfall", PathSprites, @"\tilesheetfall", ".png");
            LoadSprite("tilesheetwinter", PathSprites, @"\tilesheetwinter", ".png");


            LoadSprite("tilesheetwm", PathSprites, @"\tilesheetwm", ".png"); // tile sheet
            LoadSprite("tilesheetone", PathSprites, @"\tilesheetone", ".png"); // tile sheet
            LoadSprite("tilesheettwo", PathSprites, @"\tilesheettwo", ".png"); // tile sheet
            LoadSprite("font", PathSprites, @"\font", ".png"); // font
            LoadSprite("hero", PathSprites, @"\hero", ".png"); // hero
            /*LoadSprite("energi", PathSprites, @"\energi", ".png");*/ // energi
            LoadSprite("items", PathSprites, @"\items", ".png");

            // enemy
            LoadSprite("enemyzero", PathSprites, @"\enemyzero", ".png");
            LoadSprite("enemyone", PathSprites, @"\enemyone", ".png");
            LoadSprite("enemytwo", PathSprites, @"\enemytwo", ".png");
            LoadSprite("enemythree", PathSprites, @"\enemythree", ".png");
            LoadSprite("enemyboss", PathSprites, @"\enemyboss", ".png");

            LoadSprite("cord", PathSprites, @"\cord", ".bmp"); // tile sheet that is coordinates

        }

        private void LoadSprite(string FriendlyName, string FilePath, string FileName, string FileExtension)
        {
            // make sure resource exists
            var fullDirectory = ReadWrite.CreateIfNotExists(FilePath, FileName, FileExtension, false);
            if (!string.IsNullOrEmpty(fullDirectory))
            {
                var sprite = Sprite.Load(fullDirectory);
                MapSprites.Add(FriendlyName, sprite);
            }
            else
            {
                ReadWrite.WriteToLog(String.Format("LoadSprite - Could not load resource. FriendlyName: {0}. Root: {1}. Path: {2}. FileName: {3}. FileExtension: {4}",
                    FriendlyName, ReadWrite.GetRoot, FilePath, FileName, FileExtension));
                throw new FileLoadException("Could not Load resource");
            }
        }

        private void LoadAllMapData()
        {
            LoadMapData("worldmap", PathMapData, @"\worldmap", ".json");
            LoadMapData("mapone", PathMapData, @"\mapone", ".json"); 
            LoadMapData("maptwo", PathMapData, @"\maptwo", ".json"); 
            LoadMapData("mapthree", PathMapData, @"\mapthree", ".json"); 
            LoadMapData("mapfour", PathMapData, @"\mapfour", ".json");
            LoadMapData("mapfive", PathMapData, @"\mapfive", ".json");
            LoadMapData("mapsix", PathMapData, @"\mapsix", ".json");
            LoadMapData("mapseven", PathMapData, @"\mapseven", ".json");
            LoadMapData("mapeight", PathMapData, @"\mapeight", ".json");
            LoadMapData("mapnine", PathMapData, @"\mapnine", ".json");

        }

        private void LoadMapData(string FriendlyName, string FilePath, string FileName, string FileExtension)
        {
            // make sure resource exists
            var fullDirectory = ReadWrite.CreateIfNotExists(FilePath, FileName, FileExtension, false);
            if (!string.IsNullOrEmpty(fullDirectory))
            {
                var mapData = ReadWrite.ReadJson<LevelObj>(FilePath, FileName, FileExtension, false);
                if (mapData != null)
                {
                    MapData.Add(FriendlyName, mapData);
                }
                else
                {
                    ReadWrite.WriteToLog(String.Format("LoadMapData - Data is null. FriendlyName: {0}. Root: {1}. Path: {2}. FileName: {3}. FileExtension: {4}",
                    FriendlyName, ReadWrite.GetRoot, FilePath, FileName, FileExtension));
                    throw new FileLoadException("Could not Load resource");
                }
            }
            else
            {
                ReadWrite.WriteToLog(String.Format("LoadMapData - Could not load resource. FriendlyName: {0}. Root: {1}. Path: {2}. FileName: {3}. FileExtension: {4}",
                    FriendlyName, ReadWrite.GetRoot, FilePath, FileName, FileExtension));
                throw new FileLoadException("Could not Load resource");
            }
        }

        private void LoadMaps()
        {
            var wm = new WorldMap();
            MapMaps.Add("worldmap", wm);

            var lvl1 = new MapOne();
            MapMaps.Add("mapone", lvl1);
            var lvl2 = new MapTwo();
            MapMaps.Add("maptwo", lvl2);
            var lvl3 = new MapThree();
            MapMaps.Add("mapthree", lvl3);
            var lvl4 = new MapFour();
            MapMaps.Add("mapfour", lvl4);
            var lvl5 = new MapFive();
            MapMaps.Add("mapfive", lvl5);
            var lvl6 = new MapSix();
            MapMaps.Add("mapsix", lvl6);
            var lvl7 = new MapSeven();
            MapMaps.Add("mapseven", lvl7);
            var lvl8 = new MapEight();
            MapMaps.Add("mapeight", lvl8);

            var lvl9 = new MapNine();
            MapMaps.Add("mapnine", lvl9);

        }

        private void LoadItems()
        {
            var e = new ItemEnergi();
            MapItems.Add("energi", e);
        }

        private void LoadSettings()
        {
            Settings = ReadWrite.ReadJson<SettingsObj>(PathSettings, @"\settings", ".json");
        }

        public bool SaveSettings()
        {
             return ReadWrite.WriteJson<SettingsObj>(PathSettings, @"\settings", ".json", Settings);
        }


        #region High Score
        private void LoadHighScore()
        {
            HighScoreList = ReadWrite.ReadJson <List<HighScoreObj>> (PathSettings, @"\highscore", ".json");

            //High Score
            if (HighScoreList == null)
                HighScoreList = new List<HighScoreObj>();

            if (HighScoreList.Count < 6)
            {
                int addToFive = 5 - HighScoreList.Count;
                for (int i = 0; i < addToFive; i++)
                {
                    HighScoreList.Add(new HighScoreObj { DateTime = DateTime.Now, Handle = "Empty", TimeSpan = new TimeSpan(7, 23, 59, 59) });
                }
            }
            HighScoreList = HighScoreList.OrderBy(x => x.TimeSpan).ThenBy(y => y.DateTime).ToList();

          

        }
        public bool PlacesOnHighScore(TimeSpan TS)
        {
            return HighScoreList.Any(x => x.TimeSpan > TS);
        }
        public void PutOnHighScore(HighScoreObj HSO)
        {
            HighScoreList.Add(HSO);
            HighScoreList = HighScoreList.OrderBy(x => x.TimeSpan).ThenBy(y => y.DateTime).Take(5).ToList();
        }
        public void ResetHighScore()
        {
            HighScoreList = new List<HighScoreObj>();
            for (int i = 0; i < 5; i++)
            {
                HighScoreList.Add(new HighScoreObj { DateTime =new DateTime(2020,7,28), Handle = "Empty", TimeSpan = new TimeSpan(7, 23, 59, 59) });
            }
            SaveHighScoreList();
        }
        public bool IsNewFirstPlaceHS(TimeSpan TS)
        {
            if (HighScoreList.FirstOrDefault().TimeSpan > TS)
            {
                return true;
            }
            return false;
        }
        public List<HighScoreObj> GetHighScoreList()
        {
            return HighScoreList;
        }
        public bool SaveHighScoreList()
        {
            return ReadWrite.WriteJson<List<HighScoreObj>>(PathSettings, @"\highscore", ".json", HighScoreList);
        }
        #endregion

        internal LevelObj GetMapData(string name)
        {
            LevelObj mapData;
            if (MapData.TryGetValue(name, out mapData))
            {
                return mapData;
            }
            else
            {
                return null;
            }
        }

        internal Sprite GetSprite(string name)
        {
            Sprite sprite;
            if (MapSprites.TryGetValue(name, out sprite))
            {
                return sprite;
            }
            else
            {
                return null;
            }
        }

        public Item GetItem(string name)
        {
            Item item;
            if (MapItems.TryGetValue(name, out item))
            {
                return item;
            }
            else
            {
                return null;
            }
        }

        public Map GetMap(string name)
        {
            Map map;
            if (MapMaps.TryGetValue(name, out map))
            {
                return map;
            }
            else
            {
                return null;
            }
        }

        public SettingsObj GetSettings()
        {
            return Settings;
        }

        public bool SaveSettings(SettingsObj settingsObj)
        {

            this.Settings = settingsObj;
            return true;

            // TODO; spara settings
            // return ReadWrite.WriteJson<SettingsObj>(PathSettings, @"\settings", ".json", settingsObj);


        }

        public int RNG(int SmallNumber, int BigNumber)
        {
            if (SmallNumber > BigNumber)
                return Random.Next(BigNumber, SmallNumber);

            return Random.Next(SmallNumber, BigNumber);
        }


        #region SwitchX
        public bool IsUnderGround { get; set; } = false;
        public bool IsAboveGround { get; set; } = false;
        public bool IsMoving { get; set; } = false;
        public bool HasBeenOnTheTop { get; set; } = true;

        public bool HasBeenOnTheBottom { get; set; } = false;


        public void CheckSwitchX()
        {
            // är under jorden
            if (IsUnderGround && HasBeenOnTheTop && !IsMoving)
            {
                IsMoving = false;
                HasBeenOnTheTop = false;
                HasBeenOnTheBottom = true;

                ChangeX();
            }

            //nollställ för att kunna switcha igen
            if (IsAboveGround && HasBeenOnTheBottom && !IsMoving)
            {
                IsMoving = false;
                HasBeenOnTheTop = true;
                HasBeenOnTheBottom = false;
            }

        }


        public void ChangeX()
        {
            var a = 0;
            var b = 0;
            var c = 0;
            var idx = 0;
            Random rand = new Random();
            do
            {
                idx++;
                a = rand.Next(0, 5);
                b = rand.Next(0, 5);
                c = rand.Next(0, 5);
            } while ((a == b) || (b == c) || (a == c) || (idx > 25));

            if (idx > 25)
            {
                ValueXArray = new int[] { 2, 4, 6 };
            }
            else
            {
                ValueXArray = new int[] { PossibleValueForX[a], PossibleValueForX[b], PossibleValueForX[c] };
            }


        }

        int[] PossibleValueForX = new int[] { 2, 4, 6, 8, 10, 12 };


        int[] ValueXArray = new int[] { 2, 4, 6 };

        public int GetMyX(int id)
        {
            return ValueXArray[id-1];
        }


        #endregion


    }


}
