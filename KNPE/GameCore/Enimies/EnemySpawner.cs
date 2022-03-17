using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace KNPE
{
    class EnemySpawner: Enemy
    {
        int LastSpawn = 0; 
        Random Temp;
        public EnemySpawner()
        {
            Type = EnemyType.EnemySpawner;
        }
        public override void Init(Vector3 StartPos)
        {
            Temp = new Random((int)(StartPos.X + StartPos.Y + StartPos.Z));
            base.Init(StartPos);
        }
        public override void Update(GameTime Time)
        {
            if (!Alive) return;
            if (LastSpawn == 0)
            {
                if (Temp.Next(1000) > Game_Core.ZombieChance)
                {
                    Vector3 PlayerTarget = (Player.Collision.Center - Collision.Center);
                    if (Game_Core.LastZombieSpawn == 0 && PlayerTarget.Length() > 150)
                    {
                        Game_Core.CreateCreeper(Collision.Center);
                        LastSpawn = Game_Core.SpawnTimer;
                        Game_Core.LastZombieSpawn = Game_Core.FrequencyOfZombies;
                    }
                }
            }
            else
            {
                LastSpawn--;
            }
        }
        public override void Damage(Vector3 Position, int Amount)
        {

        }
    }
}
