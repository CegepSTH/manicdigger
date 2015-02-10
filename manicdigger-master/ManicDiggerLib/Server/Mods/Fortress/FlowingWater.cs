using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ManicDigger.Server.Mods.Fortress
{
    class FlowingWater : IMod
    {

        //###################################################
        private class WaterBox
        {
            // force de l'eau (distance max de la source)
            private const int MAX_STRENGH = 3;

            private int _x, _y, _z, _s;

            private bool _isSource;
            public int X { get { return _x; } }
            public int Y { get { return _y; } }
            public int Z { get { return _z; } }

            public bool Source { get { return _isSource; } }
            private ModManager _mod;

            private List<WaterBox> _water = new List<WaterBox>();

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

                mod.SetBlock(x, y, z, _mod.GetBlockId("Water"));
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
                bool canX = CanFlowX();
                bool canY = CanFlowY();

                // retourne le resultat final (l'eau ira dans tous les sens possible)
                return canX || canY;
            }

            private bool CanFlowZ()
            {
                string name = _mod.GetBlockName(_mod.GetBlock(_x, _y, _z - 1));
                bool canFill = false;
                if (name == "Water" || name == "Empty")
                {
                    _water.Add(new WaterBox(_x, _y, _z - 1, _s, _mod, false));
                    canFill = true;
                }
                return canFill;
            }

            private bool CanFlowX()
            {
                string[] v = new string[] {"Empty", "Water"};
                bool canP = FillPlaneSurfaceWithFluid(_x + 1, _y, _z, _s - 1, v);
                bool canN = FillPlaneSurfaceWithFluid(_x - 1, _y, _z, _s - 1, v);
                return canP || canN;
            }

            private bool CanFlowY()
            {
                string[] v = new string[] { "Empty", "Water" };
                bool canP = FillPlaneSurfaceWithFluid(_x, _y - 1, _z, _s - 1, v);
                bool canN = FillPlaneSurfaceWithFluid(_x, _y + 1, _z, _s - 1, v);
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
                    if (textures[i] == bottom)
                        return false;
                }
                if (canFill)
                {
                    _water.Add(new WaterBox(x, y, z, s, _mod, false));
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
        }

        public void Start(ModManager m)
        {
            // ajoute la Classe au mode Default (forteress)
            m.RequireMod("Default");
        }

        void Build(int player, int x, int y, int z)
        {
            if (m.GetBlockName(m.GetBlock(x, y, z)) == "Water")
            {
                WaterBox w = new WaterBox(x, y, z, 500, m, true);
            }
        }

        void Delete(int player, int x, int y, int z, int blockid)
        {
            if (WaterAround(x, y, z))
            {
                WaterBox w = new WaterBox(x, y, z, 500, m, true);
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
