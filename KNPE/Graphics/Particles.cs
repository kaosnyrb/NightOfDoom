using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace KNPE
{
    class ParticleManger
    {
        private static Particle[] ParticleList;
        public static int MAXPARTICLES = 1000;
        public static void Setup()
        {
            ParticleList = new Particle[MAXPARTICLES];
            for (int i = 0; i < MAXPARTICLES; i++)
            {
                ParticleList[i] = new Particle();
                ParticleList[i].Lifetime = 0;
            }
        }
        public static void AddParticle(int Texture, Vector3 Position, Vector3 Velocity, int Lifetime, float Size)
        {
            bool SetParticle = false;
            for (int i = 0; i < MAXPARTICLES && !SetParticle; i++)
            {
                if (ParticleList[i].Lifetime == 0)
                {
                    ParticleList[i].Position = Position;
                    ParticleList[i].Velocity = Velocity;
                    ParticleList[i].Texture = Texture;
                    ParticleList[i].Lifetime = Lifetime;
                    ParticleList[i].Size = Size;
                    SetParticle = true;
                }
            }
        }
        public static void FreeParticles()
        {
            //Antilag measures
            for (int i = 0; i < MAXPARTICLES; i += 2)
            {
                ParticleList[i].Lifetime = 0;
            }
        }
        public static void Update()
        {
            for (int i = 0; i < MAXPARTICLES; i++)
            {
                if (ParticleList[i].Lifetime > 0)
                {
                    ParticleList[i].Lifetime--;
                    ParticleList[i].Position += ParticleList[i].Velocity;
                }
            }
        }
        public static void Draw()
        {

            for (int i = 0; i < MAXPARTICLES; i++)
            {
                if (ParticleList[i].Lifetime > 0)
                {
                    //Quadmanger.RenderQuad(ParticleList[i].Texture, ParticleList[i].Position, -Camera.CameraForward, Camera.CameraUp, 100, 100);
                    Quadmanger.RenderBillboard(ParticleList[i].Texture, ParticleList[i].Position, Camera.CameraUp, ParticleList[i].Size, ParticleList[i].Size);
                }
            }
        }
        public static void ExplodeParticleEffect(Vector3 Position)
        {
            Random Rand = new Random();
            for (int i = 0; i < 50; i++)
            {
                float X = 50 - Rand.Next(100);
                X = X / 80;
                float Y = 50 - Rand.Next(100);
                Y = Y / 90;
                float Z = 50 - Rand.Next(100);
                Z = Z / 80;
                ParticleManger.AddParticle(0, Position, new Vector3(X, Y, Z), 50, 20);
            }
        }
        public static void EvilExplodeParticleEffect(Vector3 Position)
        {
            Random Rand = new Random();
            for (int i = 0; i < 15; i++)
            {
                float X = 50 - Rand.Next(100);
                X = X / 100;
                float Y = 50 - Rand.Next(100);
                Y = Y / 100;
                float Z = 50 - Rand.Next(100);
                Z = Z / 100;
                ParticleManger.AddParticle(1, Position, new Vector3(X, Y, Z), 50, 50);
            }
        }
        public static void GoodExplodeParticleEffect(Vector3 Position)
        {
            Random Rand = new Random();
            for (int i = 0; i < 15; i++)
            {
                float X = 50 - Rand.Next(100);
                X = X / 50;
                float Y = 50 - Rand.Next(100);
                Y = Y / 50;
                float Z = 50 - Rand.Next(100);
                Z = Z / 50;
                ParticleManger.AddParticle(0, Position, new Vector3(X, Y, Z), 50, 3);
            }
        }
        public static void NadeParticleEffect(Vector3 Position)
        {
            Random Rand = new Random();
            for (int i = 0; i < 50; i++)
            {
                float X = 50 - Rand.Next(100);
                X = X / 20;
                float Y = Rand.Next(50);
                Y = Y / 20;
                float Z = 50 - Rand.Next(100);
                Z = Z / 20;
                ParticleManger.AddParticle(1, Position, new Vector3(X, Y, Z), 50, 140);
            }
        }
        public static void ShieldParticleEffect(Vector3 Position)
        {
            Random Rand = new Random();
            for (int i = 0; i < 15; i++)
            {
                float X = 50 - Rand.Next(100);
                X = X / 100;
                float Y = 50 - Rand.Next(100);
                Y = Y / 100;
                float Z = 50 - Rand.Next(100);
                Z = Z / 100;
                ParticleManger.AddParticle(0, Position, new Vector3(X, Y, Z), 50, 25);
            }
        }
        public static void TrailEffect(Vector3 Position)
        {
            Random Rand = new Random();
            float X = 50 - Rand.Next(100);
            X = X / 100;
            float Y = 50 - Rand.Next(100);
            Y = Y / 100;
            float Z = 50 - Rand.Next(100);
            Z = Z / 100;
            ParticleManger.AddParticle(0, Position, new Vector3(X, Y, Z), 50, 10);
        }
        public static void IonEffect(Vector3 Position)
        {
            Random TempRand = new Random();
            for (int i = 0; i < 20; i++)
            {
                ParticleManger.AddParticle(0, Position + new Vector3(10 - TempRand.Next(20), 10 - TempRand.Next(20), 10 - TempRand.Next(20)), new Vector3(10 - TempRand.Next(20), 10 - TempRand.Next(20), 10 - TempRand.Next(20)), 100, 50);
            }
        }
        public static void RocketTrailEffect(Vector3 Position)
        {
            ParticleManger.AddParticle(0, Position, new Vector3(0, 0, 0), 10, 5);
        }
        public static void BulletEvilTrailEffect(Vector3 Position)
        {
            ParticleManger.AddParticle(0, Position, new Vector3(0, 0, 0), 3, 5);
        }
        public static void ScannerEffect(Vector3 Position, Vector3 Velocity)
        {
            ParticleManger.AddParticle(0, Position, Velocity, 20, 10);
        }
        public static void DoomLaserEffect(Vector3 Position)
        {
            Random Rand = new Random();
            for (int i = 0; i < 5; i++)
            {
                float X = 50 - Rand.Next(100);
                X = X / 50;
                float Y = 50 - Rand.Next(100);
                Y = Y / 50;
                float Z = 50 - Rand.Next(100);
                Z = Z / 50;
                ParticleManger.AddParticle(0, Position, Camera.CameraForward * 4 + new Vector3(X, Y, Z) / 5, 10, 1);
            }
        }

        public static void Wipe()
        {
            for (int i = 0; i < MAXPARTICLES; i++)
            {
                ParticleList[i].Lifetime = 0;
            }
        }
    }
    class Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public int Texture;
        public int Lifetime = 0;
        public float Size = 100;
    }
}

