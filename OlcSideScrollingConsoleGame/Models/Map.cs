using OlcSideScrollingConsoleGame.Commands;
using OlcSideScrollingConsoleGame.Models.Items;
using OlcSideScrollingConsoleGame.Models.Objects;
using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Models
{


    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
        public Sprite Sprite { get; set; }
        /// <summary>
        /// Plural for index 
        /// </summary>
        private int[] Indices { get; set; }
        private bool[] Solids { get; set; }

       
        public static ScriptProcessor Script { get { return Core.Aggregate.Instance.Script;} }

        public Map()
        {
            Width = 0;
            Height = 0;
            Name = "";
            Sprite = null;
        }

        public virtual bool PopulateDynamics(List<DynamicGameObject> listDynamicObjs)
        {
            return false;
        }
        public virtual bool OnInteraction(List<DynamicGameObject> listDynamicObjs, DynamicGameObject target, Enum.NATURE nature)
        {
            return false;
        }

        public int GetIndex(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Indices[y * Width + x];
            else
                return 0;
        }


        public bool GetSolid(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return Solids[y * Width + x];
            }
            else
            {
                 return true;
            }
        }


        public bool Create(CreateObj createObj)
        {
            Name = createObj.name;
            Sprite = createObj.sprite;
            Height = createObj.levelObj.Height;
            Width = createObj.levelObj.Width;
            Indices = createObj.levelObj.TileIndex;

            // Only use solid or not solid for now. Might want other states in the future. 
            Solids = new bool[Width * Height];
            var length = createObj.levelObj.AttributeIndex.Length;
            for (int i = 0; i < length; i++)
            {
                if (createObj.levelObj.AttributeIndex[i].Equals(0))
                {
                    Solids[i] = false;
                }
                else
                {
                    Solids[i] = true;
                }
            }

            return false;
        }

    };




    public class WorldMap : Map
    {
        public CreateObj CreateObj { get; set; }

        public WorldMap()
        {

            this.CreateObj = new CreateObj()
            {
                levelObj = Core.Aggregate.Instance.GetMapData("worldmap"),
                sprite = Core.Aggregate.Instance.GetSprite("tilesheetone"),
                name = "worldmap",
            };

            CrateFromChild();
        }

        public bool CrateFromChild()
        {
            return this.Create(CreateObj);
        }

        public override bool PopulateDynamics(List<DynamicGameObject> ListDynamicObjs)
        {

            //Placering
            float placeraPortalX = 50;
            float placeraPortalY = 3;
            float skickaTillX = 2;
            float skickaTillY = 2;
            ListDynamicObjs.Add(new Teleport(placeraPortalX, placeraPortalY, "maptwo", skickaTillX, skickaTillY)); // Placering hamna, plcering visa


            return true;
        }

        public override bool OnInteraction(List<DynamicGameObject> ListDynamicObjs, DynamicGameObject target, Enum.NATURE nature)
        {

            if (target.Name == "Teleport")
            {
                Script.AddCommand(new CommandChangeMap((target as Teleport).MapName, (target as Teleport).MapPosX, (target as Teleport).MapPosY));
            }

            return false;
        }

    }


    public class MapOne : Map
    {
        public CreateObj CreateObj { get; set; }

        public MapOne()
        {

            this.CreateObj = new CreateObj()
            {
                levelObj = Core.Aggregate.Instance.GetMapData("mapone"),
                sprite = Core.Aggregate.Instance.GetSprite("tilesheetone"),
                name = "mapone",
            };

            CrateFromChild();
        }

        public bool CrateFromChild()
        {
            return this.Create(CreateObj);
        }




        public override bool PopulateDynamics(List<DynamicGameObject> ListDynamicObjs)
        {

            //Placering
            float placeraPortalX = 50;
            float placeraPortalY = 3;
            float skickaTillX = 2;
            float skickaTillY = 3;
            // ListDynamicObjs.Add(new Teleport(placeraPortalX, placeraPortalY, "maptwo", skickaTillX, skickaTillY)); // Placering hamna, plcering visa
            ListDynamicObjs.Add(new Teleport(placeraPortalX, placeraPortalY, "worldmap", skickaTillX, skickaTillY));


            // Placera ut fiender
            DynamicGameObject g1 = new DynamicCreatureEnemyPenguin();
            ListDynamicObjs.Add(g1);
            g1.px = 11;
            g1.py = 5;
            g1.Name = "NoOne";

            DynamicGameObject g2 = new DynamicCreatureEnemyPenguin();
            ListDynamicObjs.Add(g2);
            g2.px = 14;
            g2.py = 5;
            g2.Name = "NoTwo";


            //Valross
            DynamicGameObject g3 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g3);
            g3.px = 20;
            g3.py = 13;
            g3.Name = "walrus";


            //Istapp 
            DynamicGameObject g4 = new DynamicCreatureEnemyIcicle();
            ListDynamicObjs.Add(g4);
            g4.px = 60;
            g4.py = 7;
            g4.Name = "icicle";

            // Add items
            ListDynamicObjs.Add(new DynamicItem(10, 10, Core.Aggregate.Instance.GetItem("energi")));


            return true;
        }

        public override bool OnInteraction(List<DynamicGameObject> ListDynamicObjs, DynamicGameObject target, Enum.NATURE nature)
        {

            if (target.Name == "Teleport")
            {
                if (Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted < 1)
                {
                    Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = 1;
                }
                Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 1;
                Script.AddCommand(new CommandChangeMap((target as Teleport).MapName, (target as Teleport).MapPosX, (target as Teleport).MapPosY));
            }

            return false;
        }


    }


    public class MapTwo : Map
    {

        public CreateObj CreateObj { get; set; }

        public MapTwo()
        {
            this.CreateObj = new CreateObj()
            {
                levelObj = Core.Aggregate.Instance.GetMapData("maptwo"),
                sprite = Core.Aggregate.Instance.GetSprite("tilesheetone"),
                name = "maptwo",
            };

            CrateFromChild();
        }

        public bool CrateFromChild()
        {
            return this.Create(CreateObj);
        }


        public override bool PopulateDynamics(List<DynamicGameObject> ListDynamicObjs)
        {

            //ListDynamicObjs.Add(new Teleport(50.0f, 3.0f, "mapone", 2.0f, 2.0f)); // placering portal
            ListDynamicObjs.Add(new Teleport(58.0f, 3.0f, "worldmap", 2.0f, 3.0f));


            //Valross
            DynamicGameObject g2 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g2);
            g2.px = 8;
            g2.py = 22;
            g2.Name = "walrus";

            DynamicGameObject g3 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g3);
            g3.px = 20;
            g3.py = 13;
            g3.Name = "walrus";


            return true;
        }

        public override bool OnInteraction(List<DynamicGameObject> ListDynamicObjs, DynamicGameObject target, Enum.NATURE nature)
        {

            if (target.Name == "Teleport")
            {
                if (Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted < 2)
                {
                    Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = 2;
                }
                Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 2;
                Script.AddCommand(new CommandChangeMap((target as Teleport).MapName, (target as Teleport).MapPosX, (target as Teleport).MapPosY));
            }

            return false;
        }
    }

    public class MapThree : Map
    {

        public CreateObj CreateObj { get; set; }

        public MapThree()
        {
            this.CreateObj = new CreateObj()
            {
                levelObj = Core.Aggregate.Instance.GetMapData("mapthree"),
                sprite = Core.Aggregate.Instance.GetSprite("tilesheetone"),
                name = "mapthree",
            };

            CrateFromChild();
        }

        public bool CrateFromChild()
        {
            return this.Create(CreateObj);
        }


        public override bool PopulateDynamics(List<DynamicGameObject> ListDynamicObjs)
        {

            //ListDynamicObjs.Add(new Teleport(50.0f, 3.0f, "mapone", 2.0f, 2.0f)); // placering portal
            ListDynamicObjs.Add(new Teleport(58.0f, 3.0f, "worldmap", 2.0f, 5.0f));


            return true;
        }

        public override bool OnInteraction(List<DynamicGameObject> ListDynamicObjs, DynamicGameObject target, Enum.NATURE nature)
        {

            if (target.Name == "Teleport")
            {
                if (Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted < 3)
                {
                    Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = 3;

                }
                Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 3;
                Script.AddCommand(new CommandChangeMap((target as Teleport).MapName, (target as Teleport).MapPosX, (target as Teleport).MapPosY));
            }

            return false;
        }
    }

    public class MapFour : Map
    {

        public CreateObj CreateObj { get; set; }

        public MapFour()
        {
            this.CreateObj = new CreateObj()
            {
                levelObj = Core.Aggregate.Instance.GetMapData("mapfour"),
                sprite = Core.Aggregate.Instance.GetSprite("tilesheettwo"),
                name = "mapfour",
            };

            CrateFromChild();
        }

        public bool CrateFromChild()
        {
            return this.Create(CreateObj);
        }


        public override bool PopulateDynamics(List<DynamicGameObject> ListDynamicObjs)
        {
            #region items
            // Placera ut fiender
            DynamicGameObject g1 = new DynamicCreatureEnemyPenguin();
            ListDynamicObjs.Add(g1);
            g1.px = 27;
            g1.py = 8;
            g1.Name = "BadPeng";

            DynamicGameObject g2 = new DynamicCreatureEnemyPenguin();
            ListDynamicObjs.Add(g2);
            g2.px = 38;
            g2.py = 9;
            g2.Name = "BadPeng";

            DynamicGameObject g3 = new DynamicCreatureEnemyPenguin();
            ListDynamicObjs.Add(g3);
            g3.px = 47;
            g3.py = 9;
            g3.Name = "BadPeng";


            //Valross
            DynamicGameObject g4 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g4);
            g4.px = 16;
            g4.py = 11;
            g4.Name = "walrus";

            DynamicGameObject g5 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g5);
            g5.px = 20;
            g5.py = 13;
            g5.Name = "walrus";
            DynamicGameObject g6 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g6);
            g6.px = 14;
            g6.py = 15;
            g6.Name = "walrus";
            DynamicGameObject g7 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g7);
            g7.px = 21;
            g7.py = 16;
            g7.Name = "walrus";
            DynamicGameObject g8 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g8);
            g8.px = 23;
            g8.py = 21;
            g8.Name = "walrus";
            DynamicGameObject g9 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g9);
            g9.px = 50;
            g9.py = 22;
            g9.Name = "walrus";
            DynamicGameObject g10 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g10);
            g10.px = 56;
            g10.py = 15;
            g10.Name = "walrus";
            DynamicGameObject g11 = new DynamicCreatureEnemyWalrus();
            ListDynamicObjs.Add(g11);
            g11.px = 47;
            g11.py = 15;
            g11.Name = "walrus";




            // Add items
            ListDynamicObjs.Add(new DynamicItem(8, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(9, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(10, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(11, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(12, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(13, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(14, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(15, 23, Core.Aggregate.Instance.GetItem("energi")));

            ListDynamicObjs.Add(new DynamicItem(43, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(44, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(45, 23, Core.Aggregate.Instance.GetItem("energi")));
            ListDynamicObjs.Add(new DynamicItem(46, 23, Core.Aggregate.Instance.GetItem("energi"))); 
            ListDynamicObjs.Add(new DynamicItem(47, 23, Core.Aggregate.Instance.GetItem("energi"))); 
            ListDynamicObjs.Add(new DynamicItem(48, 23, Core.Aggregate.Instance.GetItem("energi")));
            #endregion


            //ListDynamicObjs.Add(new Teleport(50.0f, 3.0f, "mapone", 2.0f, 2.0f)); // placering portal
            ListDynamicObjs.Add(new Teleport(57.5f, 3.0f, "worldmap", 2.0f, 5.0f));


            return true;
        }

        public override bool OnInteraction(List<DynamicGameObject> ListDynamicObjs, DynamicGameObject target, Enum.NATURE nature)
        {

            if (target.Name == "Teleport")
            {
                if (Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted < 4)
                {
                    Core.Aggregate.Instance.Settings.ActivePlayer.StageCompleted = 4;

                    // Flag this is final stage
                    Core.Aggregate.Instance.Settings.ActivePlayer.ShowEnd = true;
                }
                Core.Aggregate.Instance.Settings.ActivePlayer.SpawnAtWorldMap = 4;
                Script.AddCommand(new CommandChangeMap((target as Teleport).MapName, (target as Teleport).MapPosX, (target as Teleport).MapPosY));
            }

            return false;
        }
    }

}
