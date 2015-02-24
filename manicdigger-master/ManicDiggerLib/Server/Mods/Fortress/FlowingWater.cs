using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ManicDigger.Server.Mods.Fortress
{
    class FlowingWater : IMod
    {
        #region private Type

        /// <summary>
        /// Type of liquids that can flow
        /// </summary>
        private enum LiquidType
        {
            NONE,
            WATER,
            LAVA,
            WATER_SOURCE,
            LAVA_SOURCE
        }

        /// <summary>
        /// This class is use to remove liquid from a source liquid around the source position
        /// pass in the constructor will be remove
        /// </summary>
        private class LiquidRemover
        {
            private int _x, _y, _z, _s;
            private ModManager _m;

            private LiquidType _liquidType = LiquidType.NONE;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x">x position of source</param>
            /// <param name="y">y position of source</param>
            /// <param name="z">z position of source</param>
            /// <param name="s">strengh of the liquid</param>
            /// <param name="m">mod manager</param>
            /// <param name="bucket">is it remove from a bucket DIRECTLY</param>
            /// <param name="liquidType">type of liquid</param>
            public LiquidRemover(int x, int y, int z, int s, ModManager m, bool bucket, LiquidType liquidType)
            {
                if (m == null)
                    return;

                _liquidType = liquidType;
                string bName = m.GetBlockName(m.GetBlock(x, y, z));
                _x = x;
                _y = y;
                _z = z;
                _s = s;
                _m = m;


                if ((bName == "Source" || bName == "Water" || bName == "Lava" || bName == "LavaSource") && bucket)
                {
                    if (_s > 0)
                        m.SetBlock(_x, _y, _z, _m.GetBlockId("Empty"));
                }

                if (bName == "Source")
                    _s = LiquidCube.MAX_STRENGH_WATER;

                RemoveZ();
                RemoveXY();
            }

            /// <summary>
            /// Remove all around on X and Y Axes and decrement the strngh for the next remover
            /// </summary>
            private void RemoveXY()
            {
                _s--;
                Remove(_x - 1, _y, _z, _s);
                Remove(_x + 1, _y, _z, _s);
                Remove(_x, _y - 1, _z, _s);
                Remove(_x, _y + 1, _z, _s);
            }

            /// <summary>
            /// Remove liquid, set the block to an empty block
            /// </summary>
            /// <param name="x">position of the block</param>
            /// <param name="y">position of the block</param>
            /// <param name="z">position of the block</param>
            /// <param name="s">position of the block</param>
            private void Remove(int x, int y, int z, int s)
            {
                string bName = _m.GetBlockName(_m.GetBlock(x, y, z));
                if (bName == "Water")
                {
                    _m.SetBlock(x, y, z, _m.GetBlockId("Empty"));

                    if (s > 0)
                    {
                        LiquidRemover w = new LiquidRemover(x, y, z, s, _m, true, _liquidType);
                    }
                }
            }


            /// <summary>
            /// Remove liquid on Z axe
            /// </summary>
            private void RemoveZ()
            {
                string bName = "";
                do
                {
                    int tempZ = _z - 1;
                    bName = _m.GetBlockName(_m.GetBlock(_x, _y, tempZ));
                    if (bName == "Water" || bName == "Source")
                        _m.SetBlock(_x, _y, tempZ, _m.GetBlockId("Empty"));
                }
                while (bName == "Water" || bName == "Source");
            }
        }



        /// <summary>
        /// Class used to add liquid in the world (physic algo)
        /// </summary>
        private class LiquidCube
        {
            //STRENGH is how many cube the liquid will flow on X and Y axes
            // will be increment on Z axes and decrement on X and Y axes
            public const int MAX_STRENGH_WATER = 7;
            public const int MAX_STRENGH_LAVA = 3;

            public readonly int MAX_STRENGH;

            // position du block
            private int _x, _y, _z, _s;

            // position de la source
            private int _sx, _sy, _sz;

            private bool _isSource;
            private ModManager _mod;
            private LiquidType _liquidType;
            private List<LiquidCube> _liquid = new List<LiquidCube>();

            public int X { get { return _x; } }
            public int Y { get { return _y; } }
            public int Z { get { return _z; } }

            public bool Source { get { return _isSource; } }

            public void SetSource(int x, int y, int z)
            {
                _sx = x;
                _sy = y;
                _sz = z;
            }

            public LiquidType LiquidType { get { return _liquidType; } }

            /// <summary>
            /// Reprenset ONE cube of liquid, source and non-source liquid
            /// </summary>
            /// <param name="x">position</param>
            /// <param name="y">position</param>
            /// <param name="z">position</param>
            /// <param name="strength">Strength left to the liquid</param>
            /// <param name="mod">mod manager</param>
            /// <param name="isSource">is it a source of simple liquid flowing from a source</param>
            /// <param name="liquidType">type of liquid</param>
            public LiquidCube(int x, int y, int z, int strength, ModManager mod, bool isSource, LiquidType liquidType)
            {
                _liquidType = liquidType;
                string name = "";

                if (liquidType == LiquidType.WATER_SOURCE || liquidType == LiquidType.WATER)
                {
                    MAX_STRENGH = MAX_STRENGH_WATER;
                    name = (isSource) ? "Source" : "Water";
                }
                else if (liquidType == LiquidType.LAVA_SOURCE || liquidType == LiquidType.LAVA)
                {
                    MAX_STRENGH = MAX_STRENGH_LAVA;
                    name = (isSource) ? "LavaSource" : "Lava";
                }
                else
                {
                    MAX_STRENGH = 0;
                }


                _x = x;
                _y = y;
                _z = z;
                _s = strength;
                _mod = mod;
                _isSource = isSource;

                if (_s < 0)
                    _s = 0;

                if (_s > MAX_STRENGH)
                    _s = MAX_STRENGH;


                mod.SetBlock(x, y, z, _mod.GetBlockId(name));
                if (CanFlow())
                    new Thread(FlowWater).Start();
            }


            /// <summary>
            /// Check if the liquid cube can flow in any direction
            /// </summary>
            /// <returns></returns>
            private bool CanFlow()
            {

                //check on z axe
                if (CanFlowZ())
                    return true;

                //validate the strenght
                if (_s < 1)
                    return false;

                //check on x and y axes
                _s--;
                bool canX = CanFlowX();
                bool canY = CanFlowY();

                // retourne le resultat final (l'eau ira dans tous les sens possible)
                return canX || canY;
            }


            /// <summary>
            /// check if liquid cube can flow on Z axe
            /// </summary>
            /// <returns>true if it can flow</returns>
            private bool CanFlowZ()
            {
                string name = _mod.GetBlockName(_mod.GetBlock(_x, _y, _z - 1));

                bool canFill = false;

                // LAVA
                if (_liquidType == LiquidType.LAVA_SOURCE || _liquidType == LiquidType.LAVA)
                {
                    if (name == "LAVA" || name == "Empty")
                    {
                        _liquid.Add(new LiquidCube(_x, _y, _z - 1, (_s + 1) >= MAX_STRENGH ? MAX_STRENGH : _s + 1, _mod, false, LiquidType.LAVA));
                        canFill = true;
                    }
                    if (name == "Water" || name == "Source")
                    {
                        _mod.SetBlock(_x, _y, _z - 1, _mod.GetBlockId("Obsidian"));
                    }

                }
                // WATER
                else if (_liquidType == LiquidType.WATER_SOURCE || _liquidType == LiquidType.WATER)
                {
                    if (name == "Water" || name == "Empty")
                    {
                        _liquid.Add(new LiquidCube(_x, _y, _z - 1, (_s + 1) >= MAX_STRENGH ? MAX_STRENGH : _s + 1, _mod, false, LiquidType.WATER));
                        canFill = true;
                    }
                    if (name == "LavaSource" || name == "Lava")
                    {
                        _mod.SetBlock(_x, _y, _z - 1, _mod.GetBlockId("Obsidian"));
                    }

                }
                return canFill;
            }

            /// <summary>
            /// Check if the liquid cube can flow on x axe 
            /// </summary>
            /// <returns>true if it can flow</returns>
            private bool CanFlowX()
            {
                string[] v = new string[] { "Empty" };
                bool canP = FillPlaneSurfaceWithFluid(_x + 1, _y, _z, _s, v);
                bool canN = FillPlaneSurfaceWithFluid(_x - 1, _y, _z, _s, v);
                return canP || canN;
            }

            /// <summary>
            /// Check if the liquid cube can flow on y axe 
            /// </summary>
            /// <returns>true if it can flow</returns>
            private bool CanFlowY()
            {
                string[] v = new string[] { "Empty" };
                bool canP = FillPlaneSurfaceWithFluid(_x, _y - 1, _z, _s, v);
                bool canN = FillPlaneSurfaceWithFluid(_x, _y + 1, _z, _s, v);
                return canP || canN;
            }

            /// <summary>
            /// Add the liquid cube passed in parameters in a LiquidCube List to be added int the world
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <param name="s"></param>
            /// <param name="textures"></param>
            /// <returns></returns>
            private bool FillPlaneSurfaceWithFluid(int x, int y, int z, int s, params string[] textures)
            {
                bool canFill = false;
                string name = _mod.GetBlockName(_mod.GetBlock(x, y, z));
                string bottom = _mod.GetBlockName(_mod.GetBlock(x, y, z - 1));

                for (int i = 0; i < textures.Length; i++)
                {
                    if (textures[i] == name)
                    {
                        canFill = true;
                        break;
                    }
                    if (_liquidType == LiquidType.LAVA && ("Water" == name || "Source" == name))
                    {
                        _mod.SetBlock(x, y, z, _mod.GetBlockId("Obsidian"));
                    }
                    if (_liquidType == LiquidType.WATER && ("Lava" == name || "LavaSource" == name))
                    {
                        _mod.SetBlock(x, y, z, _mod.GetBlockId("Obsidian"));
                    }
                    //if (textures[i] == bottom)
                    //    return false;
                }

                if (canFill)
                {

                    LiquidType tempLiquiType = LiquidType.NONE;

                    //LAVA
                    if (_liquidType == LiquidType.LAVA || _liquidType == LiquidType.LAVA_SOURCE)
                    {
                        tempLiquiType = LiquidType.LAVA;
                    }
                    //WATER
                    else if (_liquidType == LiquidType.WATER || _liquidType == LiquidType.WATER_SOURCE)
                    {
                        tempLiquiType = LiquidType.WATER;
                    }

                    LiquidCube w = new LiquidCube(x, y, z, s, _mod, false, tempLiquiType);
                    w.SetSource(_x, _y, _z);
                    _liquid.Add(w);
                    canFill = true;
                }

                return canFill;
            }

            /// <summary>
            /// Add all liquids cubes of the list in the world
            /// </summary>
            private void FlowWater()
            {
                for (int i = 0; i < _liquid.Count; i++)
                {
                    string texture = "";
                    LiquidCube w = _liquid[i];
                    LiquidType t = w.LiquidType;

                    switch (t)
                    {
                        case LiquidType.WATER:
                            texture = "Water";
                            break;
                        case LiquidType.LAVA:
                            texture = "Lava";
                            break;
                        case LiquidType.WATER_SOURCE:
                            texture = "Source";
                            break;
                        case LiquidType.LAVA_SOURCE:
                            texture = "LavaSource";
                            break;
                    }

                    bool s = _liquid[i].Source;

                    _mod.SetBlock(w.X, w.Y, w.Z, _mod.GetBlockId(texture));
                }
            }
        }

        #endregion

        private ModManager _m;

        public void PreStart(ModManager manager)
        {
            _m = manager;

            _m.RegisterOnBlockBuild(Build);
            _m.RegisterOnBlockDelete(Delete);
            _m.RegisterOnBlockUseWithTool(UseWithTool);
            _m.RegisterOnBlockBuildOnSource(BuildOnSource);
        }

        public void Start(ModManager m)
        {
            m.RequireMod("Default");
        }

        /// <summary>
        /// event called when the player build a block of any type on a source
        /// </summary>
        void BuildOnSource(int player, int x, int y, int z)
        {
            string name = _m.GetBlockName(_m.GetBlock(x, y, z));

            LiquidType type = (name == "Source") ? LiquidType.WATER : LiquidType.LAVA;

            LiquidRemover w = new LiquidRemover(x, y, z, 50, _m, false, type);
        }

        void Build(int player, int x, int y, int z)
        {
            //if (m.GetBlockName(m.GetBlock(x, y, z - 1)) == "Source")
            //    m.SetBlock(x, y, z, m.GetBlockId("Empty"));

            if (_m.GetBlockName(_m.GetBlock(x, y, z)) == "Source" ||
                _m.GetBlockName(_m.GetBlock(x, y, z)) == "WaterBucket")
            {
                LiquidCube w = new LiquidCube(x, y, z, 500, _m, true, LiquidType.WATER_SOURCE);
            }
        }

        void Delete(int player, int x, int y, int z, int blockid)
        {
            string name = _m.GetBlockName(_m.GetBlock(x, y, z));

            if (name == "Source" || name == "Water")
            {
                _m.SetBlock(x, y, z, _m.GetBlockId("Water"));
            }
            else if (name == "Lava" || name == "LavaSource")
            {
                _m.SetBlock(x, y, z, _m.GetBlockId("Lava"));
            }

            if (LiquidAroud(x, y, z) != LiquidType.NONE)
            {
                LiquidCube w = new LiquidCube(x, y, z, 500, _m, true, LiquidAroud(x, y, z));
            }
        }

        /// <summary>
        /// Liquid will be remove if the tool is an empty bucket,
        /// Water will be added if the tool is a water bucker
        /// Lava will be added if the tool is a lava bucket
        /// </summary>
        void UseWithTool(int player, int x, int y, int z, int toolId)
        {
            if (toolId == _m.GetBlockId("WBucket"))
            {
                LiquidCube w = new LiquidCube(x, y, z, 500, _m, true, LiquidType.WATER_SOURCE);
                return;
            }
            else if (toolId == _m.GetBlockId("LavaBucket"))
            {
                LiquidCube w = new LiquidCube(x, y, z, 500, _m, true, LiquidType.LAVA_SOURCE);
                return;
            }

            if (toolId == _m.GetBlockId("EmptyBucket"))
            {
                string blockName = _m.GetBlockName(_m.GetBlock(x, y, z));
                string fillName = "Empty";
                LiquidType type = LiquidType.NONE;

                if (blockName == "Source")
                {
                    fillName = "wBucket";
                    type = LiquidType.WATER;
                }

                else if (blockName == "LavaSource")
                { 
                    fillName = "LavaBucket";
                    type = LiquidType.LAVA;
                }

                int actSlot = _m.GetActiveMaterialSlot(player);

                Console.WriteLine(fillName);
                _m.GetInventory(player).RightHand[actSlot].BlockId = _m.GetBlockId(fillName);
                LiquidRemover w = new LiquidRemover(x, y, z, 50, _m, true, type);
            }
        }

        /// <summary>
        /// check if there's liquid around the position and return the Type 
        /// return Type.NONE if there's no liquid
        /// </summary>
        private LiquidType LiquidAroud(int x, int y, int z)
        {
            List<LiquidType> types = new List<LiquidType>();

            types.Add(isLiquid(x, y, z + 1));
            types.Add(isLiquid(x + 1, y, z));
            types.Add(isLiquid(x - 1, y, z));
            types.Add(isLiquid(x, y + 1, z));
            types.Add(isLiquid(x, y - 1, z));

            LiquidType strongerType = LiquidType.NONE;

            for (int i = 0; i < types.Count; i++)
            {
                // water always stronger
                if (types[i] == LiquidType.WATER || types[i] == LiquidType.WATER_SOURCE)
                    return LiquidType.WATER;

                // lava
                if (types[i] == LiquidType.LAVA || types[i] == LiquidType.LAVA_SOURCE)
                    strongerType = LiquidType.LAVA;
            }

            // return NONE if thers's no liquid around
            return strongerType;
        }


        /// <summary>
        /// check if the cube sended in parameters is a liquid
        /// </summary>
        private LiquidType isLiquid(int x, int y, int z)
        {
            string name = _m.GetBlockName(_m.GetBlock(x, y, z));

            switch (name)
            {
                case "Water":
                    return LiquidType.WATER;
                case "Source":
                    return LiquidType.WATER_SOURCE;
                case "Lava":
                    return LiquidType.LAVA;
                case "LavaSource":
                    return LiquidType.LAVA_SOURCE;
                default:
                    return LiquidType.NONE;
            }
        }
    }
}
