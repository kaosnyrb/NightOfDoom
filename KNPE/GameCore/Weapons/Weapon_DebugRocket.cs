using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KNPE
{
    class Weapon_DebugRocket : BaseWeapon
    {
        int CoolDownValue = 15;
        Grenade[] Nades = new Grenade[10];

        public Weapon_DebugRocket()
        {
            for (int i = 0; i < Nades.Length; i++)
            {
                Nades[i] = new Grenade();
            }
        }

        public override void Update()
        {
            //DRAW
            for (int i = 0; i < Nades.Length; i++)
            {
                Nades[i].Update();
            }
            base.Update();
        }
        public override void PrimaryFire(Vector3 Position)
        {
            if (Firecooldown == 0)
            {
                RumbleCore.Rumble(0.2f, 40);
                Game_Core.FireBullet(Position + (-Camera.CameraUp * 3), Camera.CameraForward * 15, true);
                Firecooldown = CoolDownValue;
                Game1.AudManager.PlaySound("Fire", Position);
            }
        }
        public override void SecondaryFire(Vector3 Position)
        {
            if (Firecooldown == 0 && Player.GrenadeCount > 0)
            {
                Firecooldown = CoolDownValue;
                bool FoundMissle = false;
                for (int i = 0; i < Nades.Length && !FoundMissle; i++)
                {
                    if (Nades[i].Alive == false)
                    {
                        Nades[i].Fire(Position);
                        FoundMissle = true;
                        Player.GrenadeCount--;
                    }
                }
            }
        }
    }
    class Grenade
    {
        public bool Alive = false;
        int Damage = 5;
        BoundingSphere Collision = new BoundingSphere(Vector3.Zero, 3);
        Vector3 Direction = Camera.CameraForward;
        public void Fire(Vector3 StartPosition)
        {
            Game1.AudManager.PlaySound("Throw");
            Alive = true;
            Collision.Center = StartPosition;
            Direction = Camera.CameraForward;
            Direction.Normalize();
        }
        public void Update()
        {
            if (Alive)
            {
                Collision.Center += Direction * 8;
                Direction = Vector3.SmoothStep(Direction, Vector3.Down, 0.04f);
                for (int i = 0; i < Game_Core.Enimies.Length; i++)
                {
                    if (Collision.Intersects(Game_Core.Enimies[i].Collision) && Game_Core.Enimies[i].Alive)
                    {
                        Explode();
                    }
                }
                Ray Next = new Ray(Collision.Center, Direction);
                if (Level.GetIntersectingPoly(Next, 200) != Vector3.Zero)
                {
                    Explode();
                }
                RenderEntity3d_Manager.RenderModel(8, Collision.Center, Matrix.Identity);
            }
        }
        public void Explode()
        {
            Alive = false;
            Game1.AudManager.PlaySound("Explosion", Collision.Center);
            ParticleManger.NadeParticleEffect(Collision.Center);
            for (int i = 0; i < Game_Core.Enimies.Length; i++)
            {
                if ((Game_Core.Enimies[i].Collision.Center - Collision.Center).Length() < 300 && Game_Core.Enimies[i].Alive && Game_Core.Enimies[i].Type == EnemyType.Creeper)
                {
                    Game_Core.Enimies[i].Damage(Collision.Center, Damage);
                }
            }
        }
    }
}
