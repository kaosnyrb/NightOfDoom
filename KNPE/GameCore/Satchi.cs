using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace KNPE
{
    class Satchi
    {
        public Vector3 Position = new Vector3(0,0,1000);
        Vector3 Veloctiy = Vector3.Zero;
        public bool Alive = false;

        EvilBot Target = new EvilBot();
        bool HaveTarget = false;

        public void Update()
        {
            if (!Alive) return;
            if (!Target.Alive || HaveTarget == false)
            {
                HaveTarget = TargetNearestEvilBot();
            }
            if (HaveTarget)
            {
                Veloctiy = Target.Position - Position;
                Veloctiy.Normalize();
                Veloctiy *= 1.5f;
                if ((Target.Position - Position).Length() < 4)
                {
                    Target.Alive = false;
                    HaveTarget = false;
                    ParticleManger.GoodExplodeParticleEffect(Position);
                }
            }
            ParticleManger.TrailEffect(Position);
            Position += Veloctiy;
            if (Camera.Frustum.Contains(Position) == ContainmentType.Contains)
            {
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                (float)Graphics_Core.graphics.GraphicsDevice.Viewport.Width / (float)Graphics_Core.graphics.GraphicsDevice.Viewport.Height,
                10,
                10000);
                Vector3 projectedPosition = Graphics_Core.graphics.GraphicsDevice.Viewport.Project(Position, projection, Camera.GetViewMatrix(), Matrix.Identity);
                Vector2 screenPosition = new Vector2(projectedPosition.X - 16, projectedPosition.Y - 16);
                screenPosition = Vector2.Clamp(screenPosition, new Vector2(0, 0), new Vector2(1248, 688));

                SpriteManager.RenderSprite(3, screenPosition);
            }
        }
        public bool TargetNearestEvilBot()
        {
            bool Foundone = false;
            float Nearest = float.MaxValue;
            for (int i = 0; i < 50; i++)
            {
                if ((Game_Core.Spawn.Bot[i].Position - Position).Length() < Nearest && Game_Core.Spawn.Bot[i].Alive)
                {
                    Target = Game_Core.Spawn.Bot[i];
                    Nearest = (Game_Core.Spawn.Bot[i].Position - Position).Length();
                    Foundone = true;
                }
            }
            return Foundone;
        }
    }
}
