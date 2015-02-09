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
            private const int MAX_STRENGH = 6;

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
                if (canFlow())
                    new Thread(FlowWater).Start();
            }

            private bool canFlow()
            {
                Console.WriteLine("canFlow");

                //regarde vers le bas
                if (canFlowDown())
                    return true;

                // regarde si la force de l'eau est asser forte
                if (_s < 1)
                    return false;

                // regarde dans toute les direction
                bool canX = canFlowX();
                bool canY = canFlowY();

                // retourne le resultat final (l'eau ira dans tous les sens possible)
                return canX || canY;

                //// retourne le une direction possible
                //return canFlowX() || canFlowY();
            }

            private bool canFlowDown()
            {
                Console.WriteLine("canFlowDown");
                if (_mod.GetBlockName(_mod.GetBlock(_x, _y, _z - 1)) == "Empty")
                {
                    _water.Add(new WaterBox(_x, _y, _z - 1, _s, _mod));
                    return true;
                }
                
                return false;
            }

            private bool canFlowX()
            {
                Console.WriteLine("canFlowX");
                bool can = false;

                if (_mod.GetBlockName(_mod.GetBlock(_x - 1, _y, _z)) == "Empty")
                {
                    _water.Add(new WaterBox(_x -1, _y, _z, _s - 1, _mod));
                    can = true;
                }
                
                if (_mod.GetBlockName(_mod.GetBlock(_x + 1, _y, _z)) == "Empty")
                {
                    _water.Add(new WaterBox(_x + 1, _y, _z, _s - 1, _mod));
                    can = true;
                }

                return can;
            }

            private bool canFlowY()
            {
                Console.WriteLine("canFlowY");
                bool can = false;

                if (_mod.GetBlockName(_mod.GetBlock(_x, _y - 1, _z)) == "Empty")
                {
                    _water.Add(new WaterBox(_x, _y - 1, _z, _s - 1, _mod));
                    can = true;
                }

                if (_mod.GetBlockName(_mod.GetBlock(_x, _y + 1, _z)) == "Empty")
                {
                    _water.Add(new WaterBox(_x, _y + 1, _z, _s - 1, _mod));
                    can = true;
                }

                return can;
            }

            private void FlowWater()
            {
                Thread.Sleep(200);

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
            Console.WriteLine("Build");
            Update(x, y, z);
        }

        void Delete(int player, int x, int y, int z, int blockid)
        {
            Update(x, y, z - 1);
        }

        void Update(int x, int y, int z)
        {
            int waterId = m.GetBlockId("Water");
            int currentBlockId = m.GetBlock(x, y, z);
            //  area[1, 1, 1] = currentBlockId;
            int tempZ = z;
            int bId = m.GetBlock(x, y, z);

            Console.WriteLine(m.GetBlockName(bId));


            // Premiere loi, fill vers le bas 
            FillDown(waterId, x, y, ref tempZ);
        }

        /// <summary>
        /// Rempli le cube sous l'eau s'il est vide
        /// </summary>
        private bool FillDown(int waterId, int x, int y, ref int z)
        {
            Console.WriteLine("tempZ: " + z.ToString());
            if (m.GetBlockName(m.GetBlock(x, y, z)) == "Water")
            {
                WaterBox w = new WaterBox(x, y, z - 1, 20, m);
                //z--;
                //Thread.Sleep(500);
               // return SetEmptyBlock(waterId, x, y, z);
            }
            return false;
        }

        /// <summary>
        /// retourne false si le block n'a pas été setter car n'était pas vide
        /// </summary>
        private bool SetEmptyBlock(int tileId, int x, int y, int z)
        {
            Console.WriteLine(m.GetBlockName(m.GetBlock(x, y, z)));

            if (m.GetBlockName(m.GetBlock(x, y, z)) != "Empty")
                return false;

            
            m.SetBlock(x, y, z, tileId);
            return true;
        }

        private void FillPlaneArea(int tileId, ref int x, ref int y, int z)
        {
            //Console.WriteLine("FillPlaneArea");

            //for (int i = -1; i < 2; i++)
            //{
            //    for (int j = -1; j < 2; j++)
            //    {
            //        SetEmptyBlock(tileId, x + i, y + j, z);
            //        int tempZ = z;
            //        bool drop = false;
            //        int cpt = 0;
            //        do
            //        {
            //            cpt++;
            //            drop = FillDown(tileId, x, y, ref tempZ);
            //        }
            //        while (drop);

            //        if (cpt > 1)
            //            return;
            //    }
            //}
        }
    }
}
