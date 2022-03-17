using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    class Weapon_HomingMissle : BaseWeapon
    {
        int CoolDownValue = 30;
        HomingMissle[] Missles = new HomingMissle[10];

        public Weapon_HomingMissle()
        {
            for (int i = 0; i < Missles.Length; i++)
            {
                Missles[i] = new HomingMissle();
            }
        }

        public override void Update()
        {
            //DRAW
            for (int i = 0; i < Missles.Length; i++)
            {
                Missles[i].Update();
            }
            base.Update();
        }
        public override void PrimaryFire(Vector3 Position)
        {
            if (Firecooldown == 0)
            {
                RumbleCore.Rumble(0.2f, 40);
                FireMissle(FindTarget(Position),Position);
                Firecooldown = CoolDownValue;
            }
        }
        public override void SecondaryFire(Vector3 Position)
        {

        }
        public Vector3 FindTarget(Vector3 Position)
        {
            bool TargetScanner = false;
            int TargetID = -1;
            float TargetLength = float.MaxValue;
            for (int i = 0; i < Game_Core.Enimies.Length; i++)
            {
                if ((Game_Core.Enimies[i].Collision.Center - Position).Length() < TargetLength && Game_Core.Enimies[i].Alive)
                {

                    if (Camera.Frustum.Contains(Game_Core.Enimies[i].Collision.Center) != ContainmentType.Disjoint )
                    {
                        TargetID = i;
                        TargetLength = (Game_Core.Enimies[i].Collision.Center - Position).Length();
                        TargetScanner = false;
                    }
                }
            }
            for (int i = 0; i < Game_Core.Scanners.Length; i++)
            {
                if ((Game_Core.Scanners[i].Collision.Center - Position).Length() < TargetLength && Game_Core.Scanners[i].Alive)
                {

                    if (Camera.Frustum.Contains(Game_Core.Scanners[i].Collision.Center) != ContainmentType.Disjoint)
                    {
                        TargetID = i;
                        TargetLength = (Game_Core.Scanners[i].Collision.Center - Position).Length();
                        TargetScanner = true;
                    }
                }
            }

            if (TargetID != -1)
            {
                if (TargetScanner)
                {
                    return Game_Core.Scanners[TargetID].Collision.Center;
                }
                else
                {
                    return Game_Core.Enimies[TargetID].Collision.Center;
                }
            }
            return Position + (Camera.CameraForward*1000);
        }
        public void FireMissle(Vector3 Target, Vector3 Pos)
        {
            bool FoundMissle = false;
            for (int i = 0; i < Missles.Length && !FoundMissle; i++)
            {
                if (Missles[i].Alive == false)
                {
                    Missles[i].Fire(Pos, Target);
                    FoundMissle = true;
                }
            }
        }
    }
    class HomingMissle
    {
        public bool Alive = false;
        Vector3 Target;
        Vector3 Position;
        BoundingSphere Collision = new BoundingSphere(Vector3.Zero,3);
        Matrix Rotation = Matrix.Identity;
        int Damage = 10;
        int LifeTime = 200;

        public void Fire(Vector3 StartPosition, Vector3 TargetPos)
        {
            Alive = true;
            Position = StartPosition;
            Target = TargetPos;
            LifeTime = 200;
        }
        public void Update()
        {
            if (!Alive) return;
            Vector3 Speed = (Target - Position);
            Speed.Normalize();
            Position += Speed * 15;
            ParticleManger.RocketTrailEffect(Collision.Center);
            Collision.Center = Position;
            Rotation = Matrix.CreateBillboard(Position, Position - Speed, Camera.CameraUp, Speed);
            Rotation.Translation = Vector3.Zero;
            for (int i = 0; i < Game_Core.Enimies.Length; i++)
            {
                if (Collision.Intersects(Game_Core.Enimies[i].Collision) && Game_Core.Enimies[i].Alive)
                {
                    Game_Core.Enimies[i].Damage(Collision.Center,Damage);
                    ParticleManger.EvilExplodeParticleEffect(Collision.Center);
                    Alive = false;
                }
            }
            for (int i = 0; i < Game_Core.Scanners.Length; i++)
            {
                if (Collision.Intersects(Game_Core.Scanners[i].Collision) && Game_Core.Scanners[i].Alive)
                {
                    Game_Core.Scanners[i].Damage( Damage);
                    ParticleManger.EvilExplodeParticleEffect(Collision.Center);
                    Alive = false;
                }
            }
            Ray Next = new Ray(Collision.Center, Speed);
            if (Level.GetIntersectingPoly(Next, 200) != Vector3.Zero)
            {
                ParticleManger.GoodExplodeParticleEffect(Collision.Center);
                Alive = false;
                Explode();
            }
            LifeTime--;
            if (LifeTime == 0)
            {
                ParticleManger.GoodExplodeParticleEffect(Collision.Center);
                Alive = false;
                Explode();
            }
            RenderEntity3d_Manager.RenderModel(2, Position, Rotation);

        }
        public void Explode()
        {
            for (int i = 0; i < Game_Core.Enimies.Length; i++)
            {
                if ((Game_Core.Enimies[i].Collision.Center - Position).Length() < 100 && Game_Core.Enimies[i].Alive)
                {
                    Game_Core.Enimies[i].Damage(Collision.Center, Damage / 2);
                }
            }
            for (int i = 0; i < Game_Core.Scanners.Length; i++)
            {
                if ((Game_Core.Scanners[i].Collision.Center - Position).Length() < 100 && Game_Core.Scanners[i].Alive)
                {
                    Game_Core.Scanners[i].Damage(Damage / 2);
                }
            }
        }
    }
}
