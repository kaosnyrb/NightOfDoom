using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Threading;
using Ziggyware;

namespace KNPE
{
    public enum GameState
    {
        Game,
        Menu,
        Paused
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Graphics_Core GCore;
        Game_Core GameCore;
        Menu_Core MenuCore;

        public static GameState CurrentState = GameState.Menu;
        public static AudioManager AudManager;
        public static ContentManager ContentMan;

        public static bool ISXBOX = false;//Needed for Xbox Spefic code

        public static bool Quit = false;
        public Game1()
        {
            GCore = new Graphics_Core(this);
            GameCore = new Game_Core();
            MenuCore = new Menu_Core();
            Utility.Game = this;
            ContentMan = this.Content;

         //   #if XBOX
                Components.Add(new GamerServicesComponent(this));
         //   #endif
            
            AudManager = new AudioManager(this);
        }

        protected override void Initialize()
        {
            GCore.Initialize(this);
            Game_Core.Init();
            AudManager.Initialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GCore.LoadContent(this);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if ( Quit ) this.Exit();

            GCore.Update(this, gameTime);
            switch(CurrentState)
            {
                case GameState.Game:
                    GameCore.Update(gameTime);
                    break;
                case GameState.Menu:
                    MenuCore.Update(gameTime);
                    break;
            }
            AudManager.Listener.Position = Camera.CameraPosition;
            AudManager.Listener.Forward = Camera.CameraForward;
            AudManager.Listener.Up = Camera.CameraUp;
            AudManager.Listener.Velocity = Vector3.Zero;
            AudManager.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GCore.Draw(gameTime);
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
