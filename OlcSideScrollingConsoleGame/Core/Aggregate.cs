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

            LoadSettings(); // TODO: läsa settings
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

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void LoadSprites()
        {
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
            LoadMapData("mapone", PathMapData, @"\mapone", ".json"); // map one
            LoadMapData("maptwo", PathMapData, @"\maptwo", ".json"); // map two
            LoadMapData("worldmap", PathMapData, @"\worldmap", ".json"); // map two
            LoadMapData("mapthree", PathMapData, @"\mapthree", ".json"); // map three
            LoadMapData("mapfour", PathMapData, @"\mapfour", ".json");
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
            var lvl1 = new MapOne();
            MapMaps.Add("mapone", lvl1);
            var lvl2 = new MapTwo();
            MapMaps.Add("maptwo", lvl2);
            var lvl3 = new MapThree();
            MapMaps.Add("mapthree", lvl3);

            var lvl4 = new MapFour();
            MapMaps.Add("mapfour", lvl4);

            var wm = new WorldMap();
            MapMaps.Add("worldmap", wm);

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

    }


}
