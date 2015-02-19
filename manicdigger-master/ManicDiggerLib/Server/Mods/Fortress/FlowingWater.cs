using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ManicDigger.Server.Mods.Fortress
{
    class FlowingWater : IMod
    {
        private enum LiquiType
        {
            NONE,
            WATER,
            LAVA,
            WATER_SOURCE,
            LAVA_SOURCE
        }


        //###################################################
        private class WaterRemover
        {
            private int _x, _y, _z, _s;
            private ModManager _m;


            public WaterRemover(int x, int y, int z, int s, ModManager m, bool bucket)
            {
                if (m == null)
                    return;

                string bName = m.GetBlockName(m.GetBlock(x, y, z));
                _x = x;
                _y = y;
                _z = z;
                _s = s;
                _m = m;


                if ((bName == "Source" || bName == "Water") && bucket)
                {
                    if (_s > 0)
                        m.SetBlock(_x, _y, _z, _m.GetBlockId("Empty"));
                }

                if (bName == "Source")
                    _s = LiquidCube.MAX_STRENGH_WATER;

                RemoveZ();
                RemoveXY();
            }

            private void RemoveXY()
            {
                Console.WriteLine("ici");
                _s--;
                Remove(_x - 1, _y, _z, _s);
                Remove(_x + 1, _y, _z, _s);
                Remove(_x, _y - 1, _z, _s);
                Remove(_x, _y + 1, _z, _s);
            }

            private void Remove(int x, int y, int z, int s)
            {
                string bName = _m.GetBlockName(_m.GetBlock(x, y, z));
                Console.WriteLine("test " + bName);
                if (bName == "Water")
                {
                    _m.SetBlock(x, y, z, _m.GetBlockId("Empty"));

                    if (s > 0)
                    {
                        WaterRemover w = new WaterRemover(x, y, z, s, _m, true);
                    }
                }
            }

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

        private class LiquidCube
        {
            public const int MAX_STRENGH_WATER = 7;
            public const int MAX_STRENGH_LAVA = 3;

            public readonly int MAX_STRENGH;

            // position du block
            private int _x, _y, _z, _s;

            // position de la source
            private int _sx, _sy, _sz;

            private bool _isSource;
            private ModManager _mod;
            private LiquiType _liquidType;
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

            public LiquiType LiquidType { get { return _liquidType; } }

            public LiquidCube(int x, int y, int z, int strength, ModManager mod, bool isSource, LiquiType liquidType)
            {
                _liquidType = liquidType;
                string name = "";

                if (liquidType == LiquiType.WATER_SOURCE || liquidType == LiquiType.WATER)
                {
                    MAX_STRENGH = MAX_STRENGH_WATER;
                    name = (isSource) ? "Source" : "Water";
                }
                else if (liquidType == LiquiType.LAVA_SOURCE || liquidType == LiquiType.LAVA)
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

                Console.WriteLine("STRENGH: " + _s.ToString());

                mod.SetBlock(x, y, z, _mod.GetBlockId(name));
                if (CanFlow())
                    new Thread(FlowWater).Start();
            }

            private bool CanFlow()
            {
                Console.WriteLine("canFlow");

                //regarde vers le bas
                if (CanFlowZ())
                    return true;

                // regarde si la force de l'eau est asser forte
                if (_s < 1)
                    return false;

                // regarde dans toute les direction
                _s--;
                bool canX = CanFlowX();
                bool canY = CanFlowY();

                // retourne le resultat final (l'eau ira dans tous les sens possible)
                return canX || canY;
            }

            private bool CanFlowZ()
            {
                string name = _mod.GetBlockName(_mod.GetBlock(_x, _y, _z - 1));

                bool canFill = false;

                // LAVA
                if (_liquidType == LiquiType.LAVA_SOURCE || _liquidType == LiquiType.LAVA)
                {
                    if (name == "LAVA" || name == "Empty")
                    {
                        _liquid.Add(new LiquidCube(_x, _y, _z - 1, (_s + 1) >= MAX_STRENGH ? MAX_STRENGH : _s + 1, _mod, false, LiquiType.LAVA));
                        canFill = true;
                    }
                }
                // WATER
                else if (_liquidType == LiquiType.WATER_SOURCE || _liquidType == LiquiType.WATER)
                {
                    if (name == "Water" || name == "Empty")
                    {
                        _liquid.Add(new LiquidCube(_x, _y, _z - 1, (_s + 1) >= MAX_STRENGH ? MAX_STRENGH : _s + 1, _mod, false, LiquiType.WATER));
                        canFill = true;
                    }
                }

                Console.WriteLine(canFill.ToString());
                return canFill;
            }

            private bool CanFlowX()
            {
                string[] v = new string[] { "Empty" };
                bool canP = FillPlaneSurfaceWithFluid(_x + 1, _y, _z, _s, v);
                bool canN = FillPlaneSurfaceWithFluid(_x - 1, _y, _z, _s, v);
                return canP || canN;
            }

            private bool CanFlowY()
            {
                string[] v = new string[] { "Empty" };
                bool canP = FillPlaneSurfaceWithFluid(_x, _y - 1, _z, _s, v);
                bool canN = FillPlaneSurfaceWithFluid(_x, _y + 1, _z, _s, v);
                return canP || canN;
            }

            private bool FillPlaneSurfaceWithFluid(int x, int y, int z, int s, params string[] textures)
            {
                bool canFill = false;
                string name = _mod.GetBlockName(_mod.GetBlock(x, y, z));
                string bottom = _mod.GetBlockName(_mod.GetBlock(x, y, z - 1));

                Console.WriteLine("FillPlaneSurfaceWithFluid");
                for (int i = 0; i < textures.Length; i++)
                {
                    Console.WriteLine(textures[i]);
                    if (textures[i] == name)
                    {
                        canFill = true;
                        break;
                    }
                    //if (textures[i] == bottom)
                    //    return false;
                }
                Console.WriteLine("ICIIIIIIIIII");

                if (canFill)
                {
                    
                    LiquiType tempLiquiType = LiquiType.NONE;

                    //LAVA
                    if (_liquidType == LiquiType.LAVA || _liquidType == LiquiType.LAVA_SOURCE)
                    {
                        tempLiquiType = LiquiType.LAVA;
                    }
                    //WATER
                    else if (_liquidType == LiquiType.WATER || _liquidType == LiquiType.WATER_SOURCE)
                    {
                        tempLiquiType = LiquiType.WATER;
                    }

                    LiquidCube w = new LiquidCube(x, y, z, s, _mod, false, tempLiquiType);
                    w.SetSource(_x, _y, _z);
                    _liquid.Add(w);
                    canFill = true;
                }

                Console.WriteLine(canFill.ToString());
                return canFill;
            }

            private void FlowWater()
            {
                Console.WriteLine("liquid: " + _liquid.Count.ToString());
                Console.WriteLine("FlowWater");
                for (int i = 0; i < _liquid.Count; i++)
                {
                    string texture = "";
                    LiquidCube w = _liquid[i];
                    LiquiType t = w.LiquidType;

                    switch (t)
                    {
                        case LiquiType.WATER:
                            texture = "Water";
                            break;
                        case LiquiType.LAVA:
                            texture = "Lava";
                            break;
                        case LiquiType.WATER_SOURCE:
                            texture = "Source";
                            break;
                        case LiquiType.LAVA_SOURCE:
                            texture = "LavaSource";
                            break;
                    }

                    bool s = _liquid[i].Source;

                    _mod.SetBlock(w.X, w.Y, w.Z, _mod.GetBlockId(texture));
                }
            }
        }
        //###################################################

        ModManager m;


        public void PreStart(ModManager manager)
        {
            m = manager;

            // s'abonne au event Build et Delete
            m.RegisterOnBlockBuild(Build);
            m.RegisterOnBlockDelete(Delete);
            m.RegisterOnBlockUseWithTool(UseWithTool);
            m.RegisterOnBlockBuildOnSource(BuildOnSource);
        }

        public void Start(ModManager m)
        {
            // ajoute la Classe au mode Default (forteress)
            m.RequireMod("Default");
        }

        void BuildOnSource(int player, int x, int y, int z)
        {
            WaterRemover w = new WaterRemover(x, y, z, 50, m, false);
        }

        void Build(int player, int x, int y, int z)
        {
            //if (m.GetBlockName(m.GetBlock(x, y, z - 1)) == "Source")
            //    m.SetBlock(x, y, z, m.GetBlockId("Empty"));

            if (m.GetBlockName(m.GetBlock(x, y, z)) == "Source" ||
                m.GetBlockName(m.GetBlock(x, y, z)) == "WaterBucket")
            {
                LiquidCube w = new LiquidCube(x, y, z, 500, m, true, LiquiType.WATER_SOURCE);
            }
        }

        void Delete(int player, int x, int y, int z, int blockid)
        {
            if (m.GetBlockName(m.GetBlock(x, y, z)) == "Source")
                m.SetBlock(x, y, z, m.GetBlockId("Source"));
            if (WaterAround(x, y, z))
            {
                LiquidCube w = new LiquidCube(x, y, z, 500, m, true, LiquiType.WATER);
            }
        }

        void UseWithTool(int player, int x, int y, int z, int toolId)
        {
            Console.WriteLine("use with tool");
            if (toolId == m.GetBlockId("WBucket"))
            {
                LiquidCube w = new LiquidCube(x, y, z, 500, m, true, LiquiType.WATER_SOURCE);
                return;
            }
            else if (toolId == m.GetBlockId("LavaBucket"))
            {
                Console.WriteLine("use lava buvket");
                LiquidCube w = new LiquidCube(x, y, z, 500, m, true, LiquiType.LAVA_SOURCE);
                return;
            }

            if (toolId == m.GetBlockId("EmptyBucket"))
            {
                int actSlot = m.GetActiveMaterialSlot(player);
                m.GetInventory(player).RightHand[actSlot].BlockId = m.GetBlockId("WBucket");
                WaterRemover w = new WaterRemover(x, y, z, 50, m, true);

            }
        }

        private bool WaterAround(int x, int y, int z)
        {
            return
                isWater(x, y, z + 1) ||
                isWater(x + 1, y, z) ||
                isWater(x - 1, y, z) ||
                isWater(x, y + 1, z) ||
                isWater(x, y - 1, z);
        }
        private bool isWater(int x, int y, int z)
        {
            string name = m.GetBlockName(m.GetBlock(x, y, z));

            return name == "Water";
        }
    }
}
