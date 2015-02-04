using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDiggerLib.Client
{
    public static class Data
    {
        public static string GameName { get; set; }
        public static Game gameRef { get; set; }

        private static float _toolRotation = -125f;

        public static float ToolRotation
        {
            get
            {
                return _toolRotation;
            }
            set { _toolRotation = value; }
        }

        public static bool up = false;
    }
}
