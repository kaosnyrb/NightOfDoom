using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace KNPE
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Insane
    }

    public enum LevelName
    {
        Graveyard,
        Aztec,
        PirateShip
    }

    class Game_Core
    {
        public static PlayerIndex CurrentLocal = PlayerIndex.One;
        public static int SpawnTimer = 250;
        public static int ZombieChance = 997;
        public static int ZombieChanceDelta = 10;
        public static int LastZombieSpawn = 0;
        public static int FrequencyOfZombies = 125;
        public static int FrequencyOfZombiesDelta = 10;
        public static int PickupChance = 900;
        public static TimeSpan BossSpawnTime = TimeSpan.FromSeconds(60);

        public static TimeSpan EndTime = new TimeSpan(0, 10, 0);
        
        public static TimeSpan TimePaused = new TimeSpan(0, 0, 0);

        public static Player LocalPlayer = new Player();

        public static Bullet[] Bullets = new Bullet[100];
        public static Enemy[] Enimies = new Enemy[100];
        public static Scanner[] Scanners = new Scanner[10];
        public static Pickup[] Pickups = new Pickup[10];

        public static EvilBotSpawner Spawn = new EvilBotSpawner();
        Satchi Chaser = new Satchi();

        public static int Alarm = 0;

        public static bool ClearedLevel = false;

        public static bool Started = false;
        bool TimerAdjust = false;

        public static bool Paused = false;
        public int PauseCooldown = 10;

        public static Difficulty CurrentDifficulty = Difficulty.Easy;

        public static bool Defeat = false;
        public static int DefeatCooldown = 350;
        public static TimeSpan DefeatTime = TimeSpan.FromSeconds(0);

        public static bool Victory = false;
        public static int VictoryCooldown = 350;

        public static bool JETPACKCHEAT = false;

        public static LevelName Name = LevelName.Graveyard;

        public static void Init()
        {
            Random TempRand = new Random();

            for (int i = 0; i < 100; i++)
            {
                Bullets[i] = new Bullet();
            }
            for (int i = 0; i < 40; i++)
            {
                Enimies[i] = new EnemySpawner();
            }
            for (int i = 40; i < 99; i++)
            {
                Enimies[i] = new Creeper();
            }
            Enimies[99] = new Boss();
            Enimies[99].Collision.Center = new Vector3(0, 0, -1350);
            for (int i = 0; i < 10; i++)
            {
                Pickups[i] = new Pickup();
            }
            //Loads the level and sets the pos
            switch (Name)
            {
                case LevelName.Graveyard:
                    LocalPlayer.SetPosition(Level.LoadLevel("Level/Zombie", 0));
                    break;
                case LevelName.Aztec:
                    LocalPlayer.SetPosition(Level.LoadLevel("Level/Temple", 0));
                    break;
                case LevelName.PirateShip:
                    LocalPlayer.SetPosition(Level.LoadLevel("Level/Pirate", 0));
                    break;
            }
            
            //Spawn.Create(Vector3.Zero);
            //Chaser.Alive = true;
            EndTime = new TimeSpan(0, 10, 0);
            Started = false;
            Paused = false;
            Defeat = false;
            DefeatCooldown = 350;
            Victory = false;
            VictoryCooldown = 350;
            DefeatTime = TimeSpan.FromSeconds(0);
            SetDifficulty();
            Player.Health = 100;
            Player.GrenadeCount = 0;
        }
        public static void SetDifficulty()
        {
            switch (CurrentDifficulty)
            {
                case Difficulty.Easy:
                    LastZombieSpawn = 0;
                    FrequencyOfZombies = 200;
                    SpawnTimer = 300;
                    ZombieChance = 997;
                    ZombieChanceDelta = 10;
                    FrequencyOfZombiesDelta = 8;
                    PickupChance = 900;
                    BossSpawnTime = TimeSpan.FromSeconds(35);
                    break;
                case Difficulty.Medium:
                    LastZombieSpawn = 0;
                    FrequencyOfZombies = 125;
                    SpawnTimer = 250;
                    ZombieChance = 997;
                    ZombieChanceDelta = 10;
                    FrequencyOfZombiesDelta = 10;
                    PickupChance = 950;
                    BossSpawnTime = TimeSpan.FromSeconds(45);
                    break;
                case Difficulty.Insane:
                    LastZombieSpawn = 0;
                    FrequencyOfZombies = 30;
                    SpawnTimer = 50;
                    ZombieChance = 997;
                    ZombieChanceDelta = 20;
                    FrequencyOfZombiesDelta = 1;
                    PickupChance = 970;
                    BossSpawnTime = TimeSpan.FromSeconds(90);
                    break;
            }
        }
        public void Update(GameTime Time)
        {
            RumbleCore.Update(Time);
            if (!Started)
            {
                EndTime = Time.TotalRealTime + TimeSpan.FromMinutes(10);
                Started = true;
            }
            ScreenTextManager.ClearText();
            if (Guide.IsVisible)
            {
                Paused = true;
            }
            if (PauseCooldown > 0) PauseCooldown--;
            if (GamePad.GetState(CurrentLocal).Buttons.Start == ButtonState.Pressed && PauseCooldown == 0)
            {
                if (Paused)
                {
                    Graphics_Core.bloom.Visible = true;
                    //Need to add the time paused to "stop" the clock during pauses
                    EndTime += TimePaused;
                    TimePaused = TimeSpan.FromSeconds(0);
                    RumbleCore.Rumble(0, 0, 1);
                }
                Paused = !Paused;
                PauseCooldown = 20;
            }
            if (Paused)
            {
                Graphics_Core.bloom.Visible = false;
                //if (!Guide.IsVisible)
                //{
                    TimePaused += Time.ElapsedGameTime;
                //}
                RenderPaused();
                if (GamePad.GetState(CurrentLocal).Buttons.Back == ButtonState.Pressed)
                {
                    Game1.CurrentState = GameState.Menu;
                }
                return;
            }

            TimeSpan TimeLeft = EndTime - Time.TotalRealTime;

            if (Player.Health < 1 && !Defeat)
            {
                DefeatTime = TimeLeft;
                Defeat = true;
                RumbleCore.Rumble(0, 0, 1);
            }
            if (Defeat)
            {
                TimeSpan TimeLasted = TimeSpan.FromMinutes(10) - DefeatTime;
                string SecondsLasted = TimeLasted.Seconds.ToString();
                if (TimeLasted.Seconds < 10)
                {
                    SecondsLasted = "0" + SecondsLasted;
                }
                ScreenTextManager.RenderText("Game Over", new Vector2(465, 250), Color.GhostWhite);
                ScreenTextManager.RenderText("You lasted " + TimeLasted.Minutes + ":" + SecondsLasted, new Vector2(345, 350), Color.GhostWhite);

                DefeatCooldown--;
                if (DefeatCooldown == 0)
                {
                    Game1.CurrentState = GameState.Menu;
                }
                return;
            }
            if (TimeLeft < BossSpawnTime)
            {
                Enimies[99].Alive = true;
            }
            if (TimeLeft < TimeSpan.FromSeconds(1))
            {
                Victory = true;
            }
            if (Victory)
            {
                ScreenTextManager.RenderText("You Survived the Night of Doom!", new Vector2(20, 250), Color.GhostWhite);
                switch (CurrentDifficulty)
                {
                    case Difficulty.Easy:
                        ScreenTextManager.RenderText("Difficulty: Easy", new Vector2(360, 350), Color.GhostWhite);
                        break;
                    case Difficulty.Medium:
                        ScreenTextManager.RenderText("Difficulty: Medium", new Vector2(360, 350), Color.GhostWhite);
                        break;
                    case Difficulty.Insane:
                        ScreenTextManager.RenderText("Difficulty: Insane", new Vector2(360, 350), Color.GhostWhite);
                        break;
                }
                VictoryCooldown--;
                if (VictoryCooldown == 0)
                {
                    Game1.CurrentState = GameState.Menu;
                }
                return;
            }
            if (!Level.LevelLoaded)
            {
                LoadScreen.DrawLoadScreen();
                return;
            }

            Random TempRandom = new Random();
            if (TempRandom.Next(10000) > 9996)
            {
                SpawnTimeBonus();
            }
            LocalPlayer.Update(Time);
            for (int i = 0; i < Enimies.Length; i++)
            {
                Enimies[i].Update(Time);
            }
            for (int i = 0; i < Bullets.Length; i++)
            {
                Bullets[i].Update();
            }
            for (int i = 0; i < Pickups.Length; i++)
            {
                Pickups[i].Update();
            }
            if (LastZombieSpawn > 0)
            {
                LastZombieSpawn--;
            }
            int FPS = 1;
            if (Time.ElapsedRealTime.Milliseconds > 0)
            {
                FPS = (1000 / Time.ElapsedRealTime.Milliseconds);
            }

            string Seconds = TimeLeft.Seconds.ToString();
            if ( TimeLeft.Seconds < 10 )
            {
                Seconds = "0" + Seconds;
            }
            //ScreenTextManager.RenderText("Kaos_Nyrb's Propriatary Engine", new Vector2(10, 10), Color.Red);
            //ScreenTextManager.RenderText(FPS.ToString(), new Vector2(1000, 10), Color.Red);
            ScreenTextManager.RenderText(TimeLeft.Minutes.ToString() + ":", new Vector2(560, 25), Color.GhostWhite);
            ScreenTextManager.RenderText(Seconds, new Vector2(620, 25), Color.GhostWhite);
            if (TimeLeft.Seconds == 0 || TimeLeft.Seconds == 30)
            {
                if (!TimerAdjust)
                {
                    FrequencyOfZombies -= FrequencyOfZombiesDelta;
                    ZombieChance -= ZombieChanceDelta;
                    TimerAdjust = true;
                }
            }
            else
            {
                TimerAdjust = false;
            }
            SpriteManager.RenderSprite(10, new Vector2(608, 328));
            //Freaky noises
            Random TempRand = new Random();
            if ( TempRand.Next(1000) > 998 )
            {
                Game1.AudManager.PlaySound("Freaky",new Vector3(400 - TempRand.Next(800),10,400 - TempRand.Next(800)));
            }
        }
        public void RenderPaused()
        {

            SpriteManager.RenderSprite(7, Vector2.Zero);
        }
        public static void FireBullet(Vector3 Position, Vector3 Velocity, bool OwnerGood)
        {
            for (int i = 0; i < 100; i++)
            {
                if (Bullets[i].Alive == false)
                {
                    Bullets[i].Fire(Position, Velocity, OwnerGood);
                    break;
                }
            }
        }
        public static void ClearGame()
        {
            for (int i = 0; i < Bullets.Length; i++)
            {
                Bullets[i].Alive = false;
            }
            for (int i = 0; i < Enimies.Length; i++)
            {
                Enimies[i].Alive = false;
            }
        }
        public static void CreateEnemy(Vector3 Position)
        {
            for (int i = 0; i < Enimies.Length; i++)
            {
                if (Enimies[i].Alive == false)
                {
                    Enimies[i].Init(Position);
                    break;
                }
            }
        }
        public static void CreateCreeper(Vector3 Position)
        {
                for (int i = 0; i < Enimies.Length; i++)
                {
                    if (Enimies[i].Alive == false && Enimies[i].Type == EnemyType.Creeper)
                    {
                        Enimies[i].Init(Position);
                        break;
                    }
                }
        }
        public static void CreateSpawner(Vector3 Position)
        {
            for (int i = 0; i < Enimies.Length; i++)
            {
                if (Enimies[i].Alive == false && Enimies[i].Type == EnemyType.EnemySpawner)
                {
                    Enimies[i].Init(Position);
                    break;
                }
            }
        }
        public static void CreateHoverBlaster(Vector3 Position)
        {
            for (int i = 0; i < Enimies.Length; i++)
            {
                if (Enimies[i].Alive == false && Enimies[i].Type == EnemyType.HoverBlaster)
                {
                    Enimies[i].Init(Position);
                    break;
                }
            }
        }
        public static void CreateScanner(Vector3 Position, int Range)
        {
            for (int i = 0; i < Scanners.Length; i++)
            {
                if (Scanners[i].Alive == false)
                {
                    Scanners[i].Init(Position, Range);
                    break;
                }
            }
        }

        public static void SpawnTimeBonus()
        {
            Random TempRand = new Random();
            Vector3 Position = new Vector3(400 - TempRand.Next(800), 0, 400 - TempRand.Next(800));
            for (int i = 0; i < Pickups.Length; i++)
            {
                if (Pickups[i].Alive == false)
                {
                    Pickups[i].Spawn(Position, PickupType.TimeBonus);
                    break;
                }
            }
        }
        public static void SpawnPickup(Vector3 Postion)
        {
            Postion.Y = 5;
            Random TempRand = new Random();
            if (TempRand.Next(1000) > PickupChance)
            {
                for (int i = 0; i < Pickups.Length; i++)
                {
                    if (Pickups[i].Alive == false)
                    {
                        if (TempRand.Next(100) > 51)
                        {
                            Pickups[i].Spawn(Postion, PickupType.Grenade);
                        }
                        else
                        {
                            Pickups[i].Spawn(Postion, PickupType.Health);
                        }
                        break;
                    }
                }
            }
        }
    }
}
