using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace KNPE
{
    class Creeper : Enemy
    {
        public Creeper()
        {
            Type = EnemyType.Creeper;
        }
        Random Temp;
        bool Spawning = false;
        int Height = -26;
        Vector3 Target = Vector3.Zero;
        bool MiniZombie = false;
        int Model = 0;
        int Size = 0;
        int SpeakCooldown = 0;

        int DrawOffset = 0;
        public override void Init(Vector3 StartPos)
        {
            Temp = new Random((int)(StartPos.X + StartPos.Y + StartPos.Z));
            base.Init(StartPos);
            Health = 4;
            Spawning = true;
            Height = -26;
            Random TempRandom = new Random();
            if (TempRandom.Next(100) > 80 && Game_Core.Name != LevelName.PirateShip)
            {
                MiniZombie = true;
                Health = 2;
                Height = -12;
                Model = 11;
                Size = 16;
            }
            else
            {
                
                switch (Game_Core.Name)
                {
                    case LevelName.Graveyard:
                        Model = 5;
                        Size = 26;
                        DrawOffset = 0;
                        break;
                    case LevelName.Aztec:
                        Model = 14;
                        Size = 32;
                        DrawOffset = 0;
                        break;
                    case LevelName.PirateShip:
                        Model = 15;
                        Size = 30;
                        DrawOffset = -20;
                        break;
                }
                
            }

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
                    RenderEntity3d_Manager.RenderModel(Model, TempPos, Rotation);
                    if (Height > 25)
                    {
                        Spawning = false;
                    }
                    return;
                }
                Vector3 PlayerTarget = (Player.Collision.Center - Collision.Center);
                if (Temp.Next(1000) > 950 && PlayerTarget.Length() < 200 && SpeakCooldown == 0)
                {
                    switch (Game_Core.Name)
                    {
                        case LevelName.Graveyard:
                            Game1.AudManager.PlaySound("ZombieSound", Collision.Center);
                            break;
                        case LevelName.Aztec:
                            Game1.AudManager.PlaySound("WizardSound", Collision.Center);
                            break;
                        case LevelName.PirateShip:
                            Game1.AudManager.PlaySound("PirateSound", Collision.Center);
                            break;

                    }
                SpeakCooldown = 200;
                }
                if (SpeakCooldown > 0) SpeakCooldown--;
                if (PlayerTarget.Length() < 500)
                {
                    //Scuttle!
                    PlayerTarget.Normalize();
                    Velocity = PlayerTarget;
                    if (MiniZombie)
                    {
                        Velocity = Velocity / 2;
                    }
                    Velocity.Y = 0;
                    Collision.Center += Velocity;
                }
                else
                {
                    if (Target == Vector3.Zero)
                    {
                        Random TempRand = new Random();
                        Target = Game_Core.Enimies[TempRand.Next(40)].Collision.Center;
                    }
                    Vector3 WalkTarget = (Target - Collision.Center);
                    if (WalkTarget.Length() < 100)
                    {
                        Target = Vector3.Zero;
                    }
                    WalkTarget.Normalize();
                    Velocity = WalkTarget;
                    if (MiniZombie)
                    {
                        Velocity = Velocity / 2;
                    }
                    Velocity.Y = 0;
                    Collision.Center += Velocity;
                }
                if (Collision.Intersects(Player.Collision))
                {
                    Player.Health--;
                    RumbleCore.Rumble(1, 10);
                }
                Rotation = Matrix.CreateBillboard(Collision.Center, Collision.Center - Velocity, Camera.CameraUp, Velocity);
                Rotation.Translation = Vector3.Zero;
                Collision.Center.Y = Size;
                RenderEntity3d_Manager.RenderModel(Model, Collision.Center + new Vector3(0,DrawOffset,0), Rotation);
            }
        }
        public override void Damage(Vector3 Position, int Amount)
        {
            Health -= Amount;
            ParticleManger.ExplodeParticleEffect(Collision.Center);
            if (Position.Y > 35)
            {
                Health = 0;
            }
            if (Health < 1)
            {
                Alive = false;
                
                Game_Core.SpawnPickup(Collision.Center);
            }
        }
    }
}
