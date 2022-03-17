using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    class HoverBlaster: Enemy
    {
        int Guncooldown = 0;

        public HoverBlaster()
        {
            Type = EnemyType.HoverBlaster;
        }
        public override void Init(Vector3 StartPos)
        {
            
            base.Init(StartPos);
            Health = 20;
        }
        public override void Update(GameTime Time)
        {
            if (Alive)
            {
                if (Camera.Frustum.Contains(Collision.Center) == ContainmentType.Disjoint)
                {
                    return;
                }
                Vector3 PlayerTarget = (Player.Collision.Center - Collision.Center);
                if (PlayerTarget.Length() < 1000 && PlayerTarget.Length() > 200)
                {
                    PlayerTarget.Normalize();
                    //Scuttle!
                    Ray Next = new Ray(Collision.Center, PlayerTarget);
                    if (Level.GetIntersectingPoly(Next, 1000) != Vector3.Zero)
                    {
                        Velocity = Vector3.Zero;
                    }
                    else
                    {
                        Velocity = PlayerTarget;
                    }
                    Collision.Center += Velocity;
                }
                if (PlayerTarget.Length() < 200 && Guncooldown == 0)
                {
                    PlayerTarget.Normalize();
                    Game_Core.FireBullet(Collision.Center, PlayerTarget, false);
                    Guncooldown = 100;
                }
                if (Guncooldown > 0) Guncooldown--;
                if (Collision.Intersects(Player.Collision))
                {
                    Player.Health--;
                    RumbleCore.Rumble(1, 10);
                }

                Rotation = Matrix.CreateBillboard(Collision.Center, Collision.Center - PlayerTarget, Camera.CameraUp, PlayerTarget);
                Rotation.Translation = Vector3.Zero;
                RenderEntity3d_Manager.RenderModel(6, Collision.Center, Rotation);
            }
        }
        public override void Damage(Vector3 Position, int Amount)
        {
            Health -= Amount;
            if (Health < 1)
            {
                Alive = false;
                ParticleManger.ExplodeParticleEffect(Collision.Center);
            }
        }
    }
}
