using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger.Server.Mods.Fortress
{
    class FlowingWater : IMod
    {
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
            Update(x, y, z);
        }

        void Update(int x, int y, int z)
        {
            int waterId = m.GetBlockId("Water");
            int[, ,] area = new int[3, 3, 3];
            // au delete x,y,z est toujours empty
            // au build x,y,z est du type du block que l'on vient de construire

            // set le block centre avec les coordonner du block envoyer par l'action...

            int currentBlockId = m.GetBlock(x, y, z);
            //  area[1, 1, 1] = currentBlockId;

            for (int i = -1; i < 2; i++)
            {
                Console.WriteLine();
                for (int j = -1; j < 2; j++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        Console.Write((i + 1).ToString() + " " + (j + 1).ToString() + "  " + (k + 1).ToString());
                        int bId = m.GetBlock(x + k, y + j, z + i);
                        Console.WriteLine(m.GetBlockName(bId));
                        area[i + 1, j + 1, k + 1] = bId;
                    }
                }
            }

            FillDown(ref area, waterId, x, y, z);
            //if (toFillDown)
            //{
            //    m.SetBlock(x, y, z, waterId);
            //    Update(x, y, z - 1);
            //}
        }

        private void FillDown(ref int[, ,] area, int waterId, int x, int y, int z)
        {
            Console.WriteLine("FillDown");
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    Console.WriteLine("\n\n" + m.GetBlockName(area[i + 1, j + 1, 0]));
                    if (area[i + 1, j + 1, 2] == waterId && m.GetBlockName(area[i + 1, j + 1, 0]) == "Empty")
                    {
                        m.SetBlock(x + i, y + j, z + 1, waterId);
                        area[i + 1, j + 1, 1] = waterId;
                    }
                }
        }


    }
}
