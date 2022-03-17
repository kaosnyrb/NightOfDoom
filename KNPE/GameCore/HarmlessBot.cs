using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KNPE
{
    class EvilBotSpawner
    {
        public Vector3 Position = Vector3.Zero;
        int Timer = 0;
        public EvilBot[] Bot = new EvilBot[50];
        public void Create(Vector3 Pos)
        {
            Position = Pos;
            for (int i = 0; i < 50; i++)
            {
                Bot[i] = new EvilBot();
                Bot[i].Position = Position;
            }
        }
        public void Update()
        {
            if (Timer == 0)
            {
                Timer = 250;
                bool FoundBot = false;
                for (int i = 0; i < 50 && !FoundBot; i++)
                {
                    if (!Bot[i].Alive)
                    {
                        Bot[i].Alive = true;
                        Bot[i].Position = Position;
                        FoundBot = true;
                        ParticleManger.EvilExplodeParticleEffect(Position);
                    }
                }
            }
            if (Timer > 0) Timer--;
            for (int i = 0; i < 50; i++)
            {
                Bot[i].Update();
            }
        }
    }
    class EvilBot
    {
        public Vector3 Position = Vector3.Zero;
        Vector3 Veloctiy = Vector3.Zero;
        int ChangeVelocity = 0;
        public bool Alive = false;

        public void Update()
        {
            if (!Alive) return;
            if (Camera.Frustum.Contains(Position) == ContainmentType.Contains)
            {
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                (float)Graphics_Core.graphics.GraphicsDevice.Viewport.Width / (float)Graphics_Core.graphics.GraphicsDevice.Viewport.Height,
                10,
                10000);
                Vector3 projectedPosition = Graphics_Core.graphics.GraphicsDevice.Viewport.Project(Position, projection, Camera.GetViewMatrix(), Matrix.Identity);
                Vector2 screenPosition = new Vector2(projectedPosition.X - 16, projectedPosition.Y - 16);
                screenPosition = Vector2.Clamp(screenPosition, new Vector2(0, 0), new Vector2(1248, 688));
                if (ChangeVelocity == 0)
                {
                    Veloctiy = RandomDirection();
                    ChangeVelocity = 400;
                }
                else
                {
                    ChangeVelocity--;
                }
                Position += Veloctiy;
                SpriteManager.RenderSprite(2, screenPosition);
            }
        }

        private Vector3 RandomDirection()
        {
            Random TempRand = new Random();
            switch (TempRand.Next(6))
            {
                case 0:
                    return new Vector3(1, 0, 0);
                case 1:
                    return new Vector3(0, 0, 1);
                case 2:
                    return new Vector3(-1, 0, 0);
                case 3:
                    return new Vector3(0, 0, -1);
                case 4:
                    return new Vector3(0, 1, 0);
                case 5:
                    return new Vector3(0, -1, 0);
            }
            return Vector3.Zero;
        }

    }
}
