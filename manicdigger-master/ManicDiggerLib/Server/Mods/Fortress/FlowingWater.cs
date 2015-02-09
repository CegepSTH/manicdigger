using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
            Update(x, y, z - 1);
        }

        void Update(int x, int y, int z)
        {
            int waterId = m.GetBlockId("Water");
           // int[, ,] area = new int[3, 3, 3];
            // au delete x,y,z est toujours empty
            // au build x,y,z est du type du block que l'on vient de construire

            // set le block centre avec les coordonner du block envoyer par l'action...

            int currentBlockId = m.GetBlock(x, y, z);
            //  area[1, 1, 1] = currentBlockId;
            int tempZ = z;
            //for (int i = -1; i < 2; i++)
            //{
            //    Console.WriteLine();
            //    for (int j = -1; j < 2; j++)
            //    {
                    //for (int k = -1; k != 2; k++)
                  //  {
                          //Console.Write((i + 1).ToString() + " " + (j + 1).ToString() + "  " + (k + 1).ToString());
                        int bId = m.GetBlock(x , y , z );

                          Console.WriteLine(m.GetBlockName(bId));


                        // Premiere loi, fill vers le bas 
                        
                        int cpt = 0;
                        bool drop = false;
                        do
                        {
                            drop = FillDown(waterId, x , y, ref tempZ);
                            Console.WriteLine("tempZ: " + tempZ.ToString());
                            cpt++;
                        }
                        while (drop);

                        Console.WriteLine("OUT tempZ: " + tempZ.ToString());
                   
                        FillPlaneArea(waterId, ref x, ref y, ++tempZ);
                    
                        
                        // verifie la pente
                  //  }
               // }
           // }
        }

        /// <summary>
        /// Rempli le cube sous l'eau s'il est vide
        /// </summary>
        private bool FillDown(int waterId, int x, int y, ref int z)
        {
            Console.WriteLine("tempZ: " + z.ToString());
            if (m.GetBlockName(m.GetBlock(x, y, z)) == "Water")
            {
                z--;
                Thread.Sleep(500);
                return SetEmptyBlock(waterId, x, y, z);
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
