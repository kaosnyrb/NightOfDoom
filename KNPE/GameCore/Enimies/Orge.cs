using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace KNPE
{
    class Orge : Enemy
    {
        public Orge()
        {
            Type = EnemyType.Orge;
        }
        Random Temp;
        bool Spawning = false;
        int Height = -26;
        public override void Init(Vector3 StartPos)
        {
            Temp = new Random((int)(StartPos.X + StartPos.Y + StartPos.Z));
            base.Init(StartPos);
            Health = 8;
            Spawning = true;
            Height = -26;
        }
        public override void Update(GameTime Time)
        {
            if (Alive)
            {
                if (Spawning)
                {
                    Vector3 TempPos = new Vector3(Collision.Center.X, Height++, Collision.Center.Z);
                    Rotation = Matrix.CreateBillboard(Collision.Center, Collision.Center - Velocity, Camera.CameraUp, Velocity);
                    Rotation.Translation = Vector3.Zero;
                    RenderEntity3d_Manager.RenderModel(7, TempPos, Rotation);
                    if (Height > 25)
                    {
                        Spawning = false;
                    }
                    return;
                }
                Vector3 PlayerTarget = (Player.Collision.Center - Collision.Center);
                //if (PlayerTarget.Length() < 1000)
                //{
                if (Temp.Next(1000) > 950 && PlayerTarget.Length() < 100)
                {
                    Game1.AudManager.PlaySound("ZombieSound", Collision.Center);
                }
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
                Velocity.Y = 0;
                Collision.Center += Velocity;
                //}
                if (Collision.Intersects(Player.Collision))
                {
                    Player.Health--;
                    RumbleCore.Rumble(1, 10);
                }
                Rotation = Matrix.CreateBillboard(Collision.Center, Collision.Center - Velocity, Camera.CameraUp, Velocity);
                Rotation.Translation = Vector3.Zero;
                Collision.Center.Y = 26;
                RenderEntity3d_Manager.RenderModel(7, Collision.Center, Rotation);

            }
        }
        public override void Damage(Vector3 Position, int Amount)
        {
            Health -= Amount;
            if (Position.Y > 35)
            {
                Health -= Amount;
            }
            if (Health < 1)
            {
                Alive = false;
                ParticleManger.ExplodeParticleEffect(Collision.Center);
            }
        }
    }
}
