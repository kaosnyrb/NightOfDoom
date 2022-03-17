using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    class Boss : Enemy
    {
        int Firecooldown = 0;
        public Boss()
        {
            Alive = false;
        }
        public override void Update(GameTime Time)
        {
            if (Alive)
            {
                if (Collision.Center.Y < 800)
                {
                    Collision.Center.Y++;
                }
                else
                {
                    Vector3 GuitarPos = new Vector3(0, -50, 0);
                    //Attack
                    if (Firecooldown == 0)
                    {
                        Vector3 PlayerTarget = (Player.Collision.Center - (Collision.Center + GuitarPos));
                        PlayerTarget.Normalize();
                        PlayerTarget *= 20;
                        Game_Core.FireBullet(Collision.Center + GuitarPos, PlayerTarget, false);
                        Firecooldown = 30;
                    }
                    else
                    {
                        Firecooldown--;
                    }
                }
                RenderEntity3d_Manager.RenderModel(13, Collision.Center, Matrix.Identity);
            }
        }
        public override void Damage(Vector3 Position, int Amount)
        {

        }
    }
}
