using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    class Weapon_DoomLaser : BaseWeapon
    {
        int CoolDownValue = 10;

        public override void Update()
        {
            //DRAW
            base.Update();
        }
        public override void PrimaryFire(Vector3 Position)
        {
            if (Firecooldown == 0)
            {
                RumbleCore.Rumble(0.2f, 40);
                Ray DoomRay = new Ray(Position, Camera.CameraForward);
                for (int i = 0; i < Game_Core.Enimies.Length; i++)
                {
                    if (!Game_Core.Enimies[i].Alive) continue;
                    float? Test = DoomRay.Intersects(Game_Core.Enimies[i].Collision);
                    if (Test != null)
                    {
                        Game_Core.Enimies[i].Alive = false;
                        ParticleManger.EvilExplodeParticleEffect(Game_Core.Enimies[i].Collision.Center);
                    }
                }
                for (int i = 0; i < Game_Core.Scanners.Length; i++)
                {
                    if (!Game_Core.Scanners[i].Alive) continue;
                    float? Test = DoomRay.Intersects(Game_Core.Scanners[i].Collision);
                    if (Test != null)
                    {
                        Game_Core.Scanners[i].Alive = false;
                        ParticleManger.EvilExplodeParticleEffect(Game_Core.Scanners[i].Collision.Center);
                    }
                }
                SpriteManager.RenderSprite(5, new Vector2(500, 355));
                ParticleManger.DoomLaserEffect(Position);
                //ParticleManger.DoomLaserEffect(Position - Camera.CameraUp, Camera.CameraForward);
                //Firecooldown = CoolDownValue;
            }
        }
        public override void SecondaryFire(Vector3 Position)
        {

        }
    }
}