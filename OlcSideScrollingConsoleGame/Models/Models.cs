using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models
{
    public class Models
    { }
    public class CreateObj
    {
        public string fileData { get; set; }
        public Sprite sprite { get; set; }
        public string name { get; set; }
        public string fileDataJsonPath { get; set; }

        public LevelObj levelObj { get;set;}
    }

    public class LevelObj 
    {
        public int[] TileIndex { get; set; }
        public int[] AttributeIndex { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }


    public class SettingsObj
    {
        public bool GameHasStarted { get; set; }

        public int StageCompleted { get; set; }

        public int SpawnAtWorldMap { get; set; } = 1;

        public int HeroEnergi { get; set; }
        public string PassedTime { get; set; }

        public string misc { get; set; }

        public List<HighScoreObj> HighScoreList { get; set; }
        public List<SavedGameObj> SavedGames { get; set; }
        public bool ShowEnd { get; set; }
    }

    public class HighScoreObj
    {
        public int Id { get; set; }
        public string Handle { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public string Time { get; set; }
    }

    public class SavedGameObj
    {
        public int Id { get; set; }
        public string Handle { get; set; }
        // TODO: Energi
        // TODO: Tid. Bygga en egen klocka för att kunna sätta tid
    }


    public class EnergiRainObject
    {
        public bool MakeItRain { get; set; }
        public int NumberOfEnergi { get; internal set; }
        /// <summary>
        /// Utgå ifrån Horisontal position 
        /// </summary>
        public float StartPosX { get; internal set; }
        /// <summary>
        /// Utgå ifrån Vertikal position
        /// </summary>
        public float StartPosY { get; internal set; }
    }


}
