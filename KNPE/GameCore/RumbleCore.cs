using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KNPE
{
    class RumbleCore
    {
        public static float Time = 0;
        public static float Power1 = 0;
        public static float Power2 = 0;

        public static void Update(GameTime gameTime)
        {
            if (Time > 0)
            {
                Time -= gameTime.ElapsedGameTime.Milliseconds;
                GamePad.SetVibration(Game_Core.CurrentLocal, Power1, Power2);
            }
            else
            {
                GamePad.SetVibration(Game_Core.CurrentLocal, 0, 0);
            }
        }

        public static void Rumble(float inPower, float Length)
        {
            Time = Length;
            Power1 = inPower;
            Power2 = inPower;
        }
        public static void Rumble(float inPower1, float inPower2, float Length)
        {
            Time = Length;
            Power1 = inPower1;
            Power2 = inPower2;
        }
    }
}
