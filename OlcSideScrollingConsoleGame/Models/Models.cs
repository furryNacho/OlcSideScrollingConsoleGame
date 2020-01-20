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
        public int StageCompleted { get; set; }

        public int SpawnAtWorldMap { get; set; } = 1;

        public bool AudioOn { get; set; } = true;

        public SaveSlotMainObj SaveSlotsObjs { get; set; } = new SaveSlotMainObj();

        public bool ShowEnd { get; set; }
    }

    public class SaveSlotMainObj
    {
        public SaveSlot SlotOne { get; set; } = new SaveSlot() {Name = "Slot One" };
        public SaveSlot SlotTwo { get; set; } = new SaveSlot() { Name = "Slot Two" };
        public SaveSlot SlotThree { get; set; } = new SaveSlot() { Name = "Slot Three" };
    }
    public class SaveSlot
    {
        public DateTime DateTime { get; set; }
        public string Name { get; set; }
        public int AmountCompleted { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsUsed { get; set; }
        public int HeroEnergi { get; set; }

    }

    public class HighScoreObj
    {
        public string Handle { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTime DateTime { get; set; }

    }

    public class HighScoreEnterName
    {
        internal string letter;

        public string MyProperty { get; set; }
    }

    public class OptionsObj
    {
        public string Display { get; set; }
        public bool OptionIsBack { get; set; }

        public bool OptionIsSlotOne { get; set; }
        public bool OptionIsSlotTwo { get; set; }
        public bool OptionIsSlotThree { get; set; }
    }

    public class SavedGameObj
    {
        public int Id { get; set; }
        public string Handle { get; set; }
        // TODO: Energi
        // TODO: Tid. Bygga en egen klocka för att kunna sätta tid
    }

    public class KonamiObj
    {
        public void nope()
        {
            up = upUp = down = downDown = left = right = leftLeft = rightRight = AB = false;
        }

        public bool up { get; set; }
        public bool upUp { get; set; }
        public bool down { get; set; }
        public bool downDown { get; set; }
        public bool left { get; set; }
        public bool right { get; set; }
        public bool leftLeft { get; set; }
        public bool rightRight { get; set; }
        public bool AB { get; set; }

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
