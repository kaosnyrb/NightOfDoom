using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace KNPE
{
    class Scanner
    {
        public bool Alive = false;
        Vector3 Position;
        public BoundingSphere Collision = new BoundingSphere(Vector3.Zero,10);
        Vector3 Target;
        BoundingFrustum ViewFrustum;
        Matrix View;
        Matrix Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,1280/720,1, 1000);
        Matrix Rotation = Matrix.Identity;
        float ScanRate = 0;
        int MaxRange = 1000;
        int WeaponCooldown = 0;
        int Health = 20;
        public void Init(Vector3 Pos, int Range)
        {
            Alive = true;
            Position = Pos;
            Collision.Center = Pos;
            MaxRange = Range;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1280 / 720, 1, MaxRange);
            Random TempRand = new Random((int)(Pos.X + Pos.Y + Pos.Z));
            ScanRate = (float)(TempRand.NextDouble() * 25);
            Health = 20;
        }

        public void Update()
        {
            if (Camera.Frustum.Contains(Collision.Center) == ContainmentType.Disjoint)
            {
                return;
            }
            if (Health < 1)
            {
                Alive = false;
            }
            if (Alive)
            {
                Target.X = Position.X + 40 * (float)(Math.Sin(ScanRate));
                Target.Z = Position.Z + 40 * (float)(Math.Cos(ScanRate));
                Target.Y = Position.Y - 10;
                View = Matrix.CreateLookAt(Position, Target, Vector3.Up);
                ViewFrustum = new BoundingFrustum(View * Projection);
                if (ViewFrustum.Contains(Player.Collision) != ContainmentType.Disjoint)
                {
                    //The player is in our feild of view, is he hiding behide something?
                    Ray SearchRay = new Ray(Position, Player.Collision.Center);
                    if (Level.GetIntersectingPoly(SearchRay, 10000) == Vector3.Zero)
                    {
                        Game_Core.Alarm = 2000;
                        Vector3 Vel = Player.Collision.Center - Position;
                        Random TempRand = new Random();
                        Vel.Normalize();
                        Vel *= 8;
                        //Vel += new Vector3(0.5f - TempRand.Next(1), 0.5f - TempRand.Next(1), 0.5f - TempRand.Next(1));
                        if (WeaponCooldown == 0)
                        {
                            Game_Core.FireBullet(Position, Vel, false);
                            WeaponCooldown = 10;
                        }
                    }
                }
                else
                {
                    ScanRate += 0.05f;
                    ParticleManger.ScannerEffect(Position, Target - Position);
                }
                Rotation = Matrix.CreateBillboard(Position, Position + (Position - Target), Camera.CameraUp, Position - Target);
                Rotation.Translation = Vector3.Zero;
                
                RenderEntity3d_Manager.RenderModel(3, Position, Rotation);
            }
            if (WeaponCooldown > 0) WeaponCooldown--;
        }
        public void Damage(int Amount)
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
