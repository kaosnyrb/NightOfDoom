using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ziggyware;

namespace KNPE
{
    class Menu_Core
    {
        public bool PreMenu = true;
        public bool HaveDevice = false;
        public bool PlayingMusic = false;
        public int Selection = 0;
        public int SelectionCooldown = 0;
        public int AdvertCooldown = 0;
        public int UnlockCooldown = 0;
        public int HelpCooldown = 0;

        public Menu_Core()
        {

        }

        public void Update(GameTime Time)
        {
            Graphics_Core.bloom.Visible = false;
            ScreenTextManager.ClearText();
            RumbleCore.Rumble(0, 0, 1);
            if (AdvertCooldown > 0)
            {
                SpriteManager.RenderSprite(9, Vector2.Zero);
                AdvertCooldown--;
                return;
            }
            if (HelpCooldown > 0)
            {
                SpriteManager.RenderSprite(15, Vector2.Zero);
                HelpCooldown--;
                return;
            }
            if (Time.TotalGameTime.Seconds < 5 && PreMenu)
            {
                SpriteManager.RenderSprite(1, Vector2.Zero);
                return;
            }
            if (PreMenu)
            {
                if (!PlayingMusic)
                {
                    Game1.AudManager.PlaySound("PlayMusic");
                    PlayingMusic = true;
                }
                #if XBOX
                    //Render splash + press A to start
                    SpriteManager.RenderSprite(6, Vector2.Zero);
                    CheckPlayer();
                ScreenTextManager.RenderText("Press A to Start", new Vector2(575, 500), Color.Red);
                #else
                PreMenu = false;
                #endif
            }
            else
            {
                SpriteManager.RenderSprite(6, Vector2.Zero);

                if (UnlockCooldown > 0)
                {
                    SpriteManager.RenderSprite(14, Vector2.Zero);
                    UnlockCooldown--;
                    return;
                }

                ScreenTextManager.RenderText("Start Game", new Vector2(575, 350), Color.Red);
                switch (Game_Core.CurrentDifficulty)
                {
                    case Difficulty.Easy:
                        ScreenTextManager.RenderText("Difficulty: Easy", new Vector2(575, 400), Color.Red);
                        break;
                    case Difficulty.Medium:
                        ScreenTextManager.RenderText("Difficulty: Medium", new Vector2(575, 400), Color.Red);
                        break;
                    case Difficulty.Insane:
                        ScreenTextManager.RenderText("Difficulty: Insane", new Vector2(575, 400), Color.Red);
                        break;
                }
                switch (Game_Core.Name)
                {
                    case LevelName.Graveyard:
                        ScreenTextManager.RenderText("Level: Graveyard", new Vector2(575, 450), Color.Red);
                        break;
                    case LevelName.Aztec:
                        ScreenTextManager.RenderText("Level: Aztec", new Vector2(575, 450), Color.Red);
                        break;
                    case LevelName.PirateShip:
                        ScreenTextManager.RenderText("Level: Pirate Ship", new Vector2(575, 450), Color.Red);
                        break;

                }
                ScreenTextManager.RenderText("How To Play", new Vector2(575, 500), Color.Red);
                if (Guide.IsTrialMode)
                {
                    ScreenTextManager.RenderText("Purchase Full Game, Only 80 points!", new Vector2(575, 550), Color.Red);
                }
                else
                {            
                    ScreenTextManager.RenderText("Other Kaos Nyrb Studio Titles", new Vector2(575, 550), Color.Red);
                }
//                ScreenTextManager.RenderText("Other Kaos Nyrb Studio Titles", new Vector2(575, 450), Color.Red);
                if (Player.ViewInvert)
                {
                    ScreenTextManager.RenderText("View Invert: On", new Vector2(575, 600), Color.Red);
                }
                else
                {
                    ScreenTextManager.RenderText("View Invert: Off", new Vector2(575, 600), Color.Red);
                }

                ScreenTextManager.RenderText("Quit", new Vector2(575, 650), Color.Red);



                if (GamePad.GetState(Game_Core.CurrentLocal).Buttons.Y == ButtonState.Pressed &&
                    GamePad.GetState(Game_Core.CurrentLocal).Buttons.X == ButtonState.Pressed &&
                    GamePad.GetState(Game_Core.CurrentLocal).Triggers.Left > 0.5f)
                {
                    Game_Core.JETPACKCHEAT = true;
                    Game1.AudManager.PlaySound("ZombieSound");
                }
                //Menu Navigation
                if (SelectionCooldown == 0)
                {
                    if (GamePad.GetState(Game_Core.CurrentLocal).DPad.Down == ButtonState.Pressed ||
                        GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.Y < -0.3f)
                    {
                        Game1.AudManager.PlaySound("Click");
                        if (Selection < 6)
                        {
                            Selection++;
                        }
                        else
                        {
                            Selection = 0;
                        }
                        SelectionCooldown = 10;
                    }
                    if (GamePad.GetState(Game_Core.CurrentLocal).DPad.Up == ButtonState.Pressed ||
                       GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.Y > 0.3f)
                    {
                        Game1.AudManager.PlaySound("Click");
                        if (Selection > 0)
                        {
                            Selection--;
                        }
                        else
                        {
                            Selection = 6;
                        }
                        SelectionCooldown = 10;
                    }
                    if (GamePad.GetState(Game_Core.CurrentLocal).Buttons.A == ButtonState.Pressed)
                    {
                        SelectionCooldown = 10;
                        Game1.AudManager.PlaySound("Click");
                        switch (Selection)
                        {
                            case 0:
                                if (!Guide.IsVisible)
                                {
                                    Game_Core.Init();
                                    Game1.CurrentState = GameState.Game;
                                    Graphics_Core.bloom.Visible = true;
                                }
                                break;
                            case 1:
                                if (!Guide.IsVisible)
                                {
                                    switch (Game_Core.CurrentDifficulty)
                                    {
                                        case Difficulty.Easy:
                                            Game_Core.CurrentDifficulty = Difficulty.Medium;
                                            break;
                                        case Difficulty.Medium:
                                            Game_Core.CurrentDifficulty = Difficulty.Insane;
                                            break;
                                        case Difficulty.Insane:
                                            Game_Core.CurrentDifficulty = Difficulty.Easy;
                                            break;

                                    }
                                }
                                break;
                            case 2:
                                if (!Guide.IsVisible)
                                {
                                    if (Guide.IsTrialMode)
                                    {
                                        UnlockCooldown = 150;
                                    }
                                    else
                                    {
                                        switch (Game_Core.Name)
                                        {
                                            case LevelName.Graveyard:
                                                Game_Core.Name = LevelName.Aztec;
                                                break;
                                            case LevelName.Aztec:
                                                Game_Core.Name = LevelName.PirateShip;
                                                break;
                                            case LevelName.PirateShip:
                                                Game_Core.Name = LevelName.Graveyard;
                                                break;
                                        }
                                    }
                                }
                                break;
                            case 3:
                                HelpCooldown = 250;
                                break;
                            case 4:
                                if (Guide.IsTrialMode)
                                {
                                    if (!Guide.IsVisible)
                                    {
                                        if (Gamer.SignedInGamers[Game_Core.CurrentLocal].IsSignedInToLive && Gamer.SignedInGamers[Game_Core.CurrentLocal].Privileges.AllowPurchaseContent)
                                        {
                                            Guide.ShowMarketplace(Game_Core.CurrentLocal);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!Guide.IsVisible)
                                    {
                                        AdvertCooldown = 250;
                                    }
                                }
                                break;
                            case 5:
                                if (!Guide.IsVisible)
                                {
                                    Player.ViewInvert = !Player.ViewInvert;
                                }
                                break;
                            case 6:
                                if (!Guide.IsVisible)
                                {
                                    Game1.Quit = true;
                                }
                                break;
                        }
                    }
                }
                else
                {
                    SelectionCooldown--;
                }

                SpriteManager.RenderSprite(8, new Vector2(500, 330 + (50 * Selection)));

            }
        }
        //-----------------------------
        // Press A to Start check
        public void CheckPlayer()
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
            {
                if (Gamer.SignedInGamers[PlayerIndex.One] != null && Gamer.SignedInGamers[PlayerIndex.One].IsSignedInToLive && !Gamer.SignedInGamers[PlayerIndex.One].IsGuest)
                {
                    Game_Core.CurrentLocal = PlayerIndex.One;
                    PreMenu = false;
                    SelectionCooldown = 15;
                }
                else
                {
                    Guide.ShowSignIn(1, true);
                }
            }
            if (GamePad.GetState(PlayerIndex.Two).IsButtonDown(Buttons.A))
            {
                if (Gamer.SignedInGamers[PlayerIndex.Two] != null && Gamer.SignedInGamers[PlayerIndex.Two].IsSignedInToLive && !Gamer.SignedInGamers[PlayerIndex.Two].IsGuest)
                {
                    Game_Core.CurrentLocal = PlayerIndex.Two;
                    PreMenu = false;
                    SelectionCooldown = 15;
                }
                else
                {
                    Guide.ShowSignIn(1, true);
                }
            }
            if (GamePad.GetState(PlayerIndex.Three).IsButtonDown(Buttons.A))
            {
                if (Gamer.SignedInGamers[PlayerIndex.Three] != null && Gamer.SignedInGamers[PlayerIndex.Three].IsSignedInToLive && !Gamer.SignedInGamers[PlayerIndex.Three].IsGuest)
                {
                    Game_Core.CurrentLocal = PlayerIndex.Three;
                    PreMenu = false;
                    SelectionCooldown = 15;
                }
                else
                {
                    Guide.ShowSignIn(1, true);
                }
            }
            if (GamePad.GetState(PlayerIndex.Four).IsButtonDown(Buttons.A))
            {
                if (Gamer.SignedInGamers[PlayerIndex.Four] != null && Gamer.SignedInGamers[PlayerIndex.Four].IsSignedInToLive && !Gamer.SignedInGamers[PlayerIndex.Four].IsGuest)
                {
                    Game_Core.CurrentLocal = PlayerIndex.Four;
                    PreMenu = false;
                    SelectionCooldown = 15;
                }
                else
                {
                    Guide.ShowSignIn(1, true);
                }
            }
        }
    }

    public class SaveFile
    {
        public int Level;
    }

}
