using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ManicDigger.Server.Mods.Fortress
{
    class FlowingWater : IMod
    {

        private class WaterBox
        {
            // force de l'eau (distance max de la source)
            private const int MAX_STRENGH = 25;

            private int _x, _y, _z, _s;
            public int X { get { return _x; } }
            public int Y { get { return _y; } }
            public int Z { get { return _z; } }
            private ModManager _mod;

            private List<WaterBox> _water = new List<WaterBox>();

            public WaterBox(int x, int y, int z, int strength, ModManager mod)
            {
                _x = x;
                _y = y;
                _z = z;
                _s = strength;
                _mod = mod;

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
                if (CanFlowDown())
                    return true;

                // regarde si la force de l'eau est asser forte
                if (_s < 1)
                    return false;

                // regarde dans toute les direction
                bool canX = CanFlowX();
                bool canY = CanFlowY();

                // retourne le resultat final (l'eau ira dans tous les sens possible)
                //return canX || canY;

                //// retourne le une direction possible
                return CanFlowX() || CanFlowY();
            }

            private bool CanFlowDown()
            {
                string name = _mod.GetBlockName(_mod.GetBlock(_x, _y, _z - 1));
                Console.WriteLine("canFlowDown");
                if (name == "Empty" || name == "Water")
                {
                    Console.WriteLine("add");
                    _water.Add(new WaterBox(_x, _y, _z - 1, _s, _mod));
                    return true;
                }

                return false;
            }

            private bool CanFlowX()
            {
                Console.WriteLine("canFlowX");
                bool can = false;

                string name = _mod.GetBlockName(_mod.GetBlock(_x - 1, _y, _z));

                if (name == "Empty")
                {
                    _water.Add(new WaterBox(_x - 1, _y, _z, _s - 1, _mod));
                    can = true;
                }

                name = _mod.GetBlockName(_mod.GetBlock(_x + 1, _y, _z));
                if (name == "Empty")
                {
                    _water.Add(new WaterBox(_x + 1, _y, _z, _s - 1, _mod));
                    can = true;
                }

                return can;
            }

            private bool CanFlowY()
            {
                Console.WriteLine("canFlowY");
                bool can = false;

                string name = _mod.GetBlockName(_mod.GetBlock(_x, _y - 1, _z));

                if (name == "Empty")
                {
                    _water.Add(new WaterBox(_x, _y - 1, _z, _s - 1, _mod));
                    can = true;
                }

                name = _mod.GetBlockName(_mod.GetBlock(_x, _y + 1, _z));
                if (name == "Empty")
                {
                    _water.Add(new WaterBox(_x, _y + 1, _z, _s - 1, _mod));
                    can = true;
                }

                return can;
            }

            private void FlowWater()
            {
                Console.WriteLine("FlowWater");
                for (int i = 0; i < _water.Count; i++)
                {
                    WaterBox w = _water[i];
                    _mod.SetBlock(w.X, w.Y, w.Z, _mod.GetBlockId("Water"));

                }
            }
        }
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
                WaterBox w = new WaterBox(x, y, z, 200, m);
            }
        }

        void Delete(int player, int x, int y, int z, int blockid)
        {
            if (WaterAround(x, y, z))
            {
                WaterBox w = new WaterBox(x, y, z, 200, m);
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
