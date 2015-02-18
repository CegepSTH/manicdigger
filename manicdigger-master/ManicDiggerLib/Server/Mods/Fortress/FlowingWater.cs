using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ManicDigger.Server.Mods.Fortress
{
    class FlowingWater : IMod
    {

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
                    _s = WaterBox.MAX_STRENGH;

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
                    int tempZ = _z-1;
                    bName = _m.GetBlockName(_m.GetBlock(_x, _y, tempZ));
                    if (bName == "Water" || bName == "Source")
                        _m.SetBlock(_x,_y, tempZ, _m.GetBlockId("Empty"));
                }
                while(bName == "Water" || bName == "Source");
            }
        }

        private class WaterBox
        {
            // force de l'eau (distance max de la source)
            public const int MAX_STRENGH = 5;

            // position du block
            private int _x, _y, _z, _s;

            // position de la source
            private int _sx, _sy, _sz;

            private bool _isSource;
            public int X { get { return _x; } }
            public int Y { get { return _y; } }
            public int Z { get { return _z; } }

            public bool Source { get { return _isSource; } }
            private ModManager _mod;

            private List<WaterBox> _water = new List<WaterBox>();

            public void SetSource(int x, int y, int z)
            {
                _sx = x;
                _sy = y;
                _sz = z;
            }

            public WaterBox(int x, int y, int z, int strength, ModManager mod, bool isSource)
            {
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

                string name = _isSource ? "Source" : "Water";
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
                Console.WriteLine("x: {0}   y: {1}   z: {2}" , _x,_y,_z);
                string name = _mod.GetBlockName(_mod.GetBlock(_x, _y, _z - 1));
                Console.WriteLine(name);
                bool canFill = false;
                if (name == "Water" || name == "Empty")
                {
                    _water.Add(new WaterBox(_x, _y, _z - 1, _s, _mod, false));
                    canFill = true;
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

                for (int i = 0; i < textures.Length; i++)
                {
                    if (textures[i] == name)
                    {
                        canFill = true;
                        break;
                    }
                    //if (textures[i] == bottom)
                    //    return false;
                }
                if (canFill)
                {
                    WaterBox w = new WaterBox(x, y, z, s, _mod, false);
                    w.SetSource(_x, _y, _z);
                    _water.Add(w);
                    canFill = true;
                }

                return canFill;
            }

            private void FlowWater()
            {
                Console.WriteLine("FlowWater");
                for (int i = 0; i < _water.Count; i++)
                {
                    WaterBox w = _water[i];
                    string block = (_water[i].Source) ? "WaterSource" : "Water";
                    _mod.SetBlock(w.X, w.Y, w.Z, _mod.GetBlockId(block));

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
                WaterBox w = new WaterBox(x, y, z, 500, m, true);
            }
        }

        void Delete(int player, int x, int y, int z, int blockid)
        {
            if (m.GetBlockName(m.GetBlock(x, y, z)) == "Source")
                m.SetBlock(x, y, z, m.GetBlockId("Source"));
            if (WaterAround(x, y, z))
            {
                WaterBox w = new WaterBox(x, y, z, 500, m, true);
            }
        }

        void UseWithTool(int player, int x, int y, int z, int toolId)
        {
            if (toolId == 176)
            {
                WaterBox w = new WaterBox(x, y, z, 500, m, true);
                return;
            }
            Console.WriteLine(m.GetBlockName(toolId));
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
