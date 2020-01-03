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
        public ScriptProcessor Script { get; set; }
        private Random Random {get;set;} = new Random();

        public SettingsObj Settings { get; set; }

        internal void Load(Program game)
        {
            ThisGame = game;
            ReadWrite = new ReadWrite();
            LoadSprites();
            LoadAllMapData();
            LoadMaps();
            LoadItems();
            Script = new ScriptProcessor();

            //LoadSettings(); // TODO: läsa settings
            Settings = new SettingsObj();
        }

        private void LoadSprites()
        {
            LoadSprite("tilesheetone", PathSprites, @"\tilesheetone", ".png"); // tile sheet
            LoadSprite("font", PathSprites, @"\font", ".png"); // font
            LoadSprite("hero", PathSprites, @"\hero", ".png"); // hero
            /*LoadSprite("energi", PathSprites, @"\energi", ".png");*/ // energi
            LoadSprite("items", PathSprites, @"\items", ".png");
            /*LoadSprite("enemyzero", PathSprites, @"\enemyzero", ".png");*/ // enemy
            LoadSprite("enemyzero", PathSprites, @"\enemyone", ".png");

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
