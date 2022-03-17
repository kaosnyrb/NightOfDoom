using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace KNPE
{
    class Player
    {
        Vector3 Velocity = new Vector3(0,0,0);
        public static BoundingSphere Collision = new BoundingSphere(new Vector3(0,0,1),30);
        Vector3 cameraPosition;
        float YRotation = 0;
        
        float ViewOffset = 0;

        Matrix PlayerRotation;

        bool OnGround = false;

        public static bool ViewInvert = false;
        
        float MoveSpeed = 3.0f;
        Vector3 Gravity = new Vector3(0, -4, 0);

        Vector3 JumpPower = new Vector3(0, 0.5f, 0);
        float JumpCooldown = 0;
        int DoorCooldown = 0;
        int Switchcooldown = 0;

        int FallTimer = 0;
        BaseWeapon CurrentWeapon = new Weapon_DebugRocket();

        public static int Health = 100;
        public static int GrenadeCount = 0;

        public Player()
        {
            Camera.CreateLookAt(new Vector3(0, 0, 0), Collision.Center);
        }

        public void Update(GameTime Time)
        {
            if (OnGround) MoveSpeed = 3.0f;
            else MoveSpeed = 1.0f;

            PlayerRotation = Matrix.CreateFromYawPitchRoll(YRotation, 0, 0);
            PlayerRotation.Up = Camera.CameraUp;
            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.Y > 0.2f)
            {
                Vector3 Temp = Camera.CameraForward;
                Temp.Y = 0;
                Temp.Normalize();
                Velocity += Temp * MoveSpeed * GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.Y;
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.Y < -0.2f)
            {
                Vector3 Temp = Camera.CameraForward;
                Temp.Y = 0;
                Temp.Normalize();
                Velocity -= Temp * MoveSpeed * -GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.Y;
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.X > 0.2f)
            {
                Vector3 Temp = Camera.CameraRight;
                Temp.Y = 0;
                Temp.Normalize();
                Velocity += Temp * MoveSpeed * GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.X;
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.X < -0.2f)
            {
                Vector3 Temp = Camera.CameraRight;
                Temp.Y = 0;
                Temp.Normalize();
                Velocity -= Temp * MoveSpeed * -GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Left.X;
            }

            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.X < -0.1f)
            {
                YRotation += (0.02f * Camera.CameraUp.Y) * -(GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.X* 1.5f);
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.X > 0.1f)
            {
                YRotation -= (0.02f * Camera.CameraUp.Y) * (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.X * 1.5f);
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.Y < -0.1f && ViewOffset > -25)
            {
                if (ViewInvert)
                {
                    ViewOffset -= 0.3f * (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.Y * 1.5f);
                }
                else
                {
                    ViewOffset += 0.3f * (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.Y * 1.5f);
                }
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.Y > 0.1f && ViewOffset < 20)
            {
                if (ViewInvert)
                {
                    ViewOffset += 0.3f * -(GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.Y * 1.5f);
                }
                else
                {
                    ViewOffset -= 0.3f * -(GamePad.GetState(Game_Core.CurrentLocal).ThumbSticks.Right.Y * 1.5f);
                }
            }
            //if (GamePad.GetState(Game_Core.CurrentLocal).IsButtonDown(Buttons.A) && OnGround)
            //{
            //    JumpCooldown = 15;
            //    OnGround = false;
            //    FallTimer = -100;
            //}
            if (GamePad.GetState(Game_Core.CurrentLocal).IsButtonDown(Buttons.X) && Switchcooldown == 0 && OnGround)
            {
                //Camera.CameraUp = -Camera.CameraUp;
                //Switchcooldown = 50;
                //RumbleCore.Rumble(0.2f,0, 500);
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).IsButtonDown(Buttons.RightShoulder) || 
                GamePad.GetState(Game_Core.CurrentLocal).IsButtonDown(Buttons.LeftShoulder))
            {
                CurrentWeapon.SecondaryFire(Collision.Center);
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).Triggers.Right > 0.2f )
            {
                CurrentWeapon.PrimaryFire(Collision.Center);
            }
            if (GamePad.GetState(Game_Core.CurrentLocal).Triggers.Left > 0.2f && Game_Core.JETPACKCHEAT)
            {
                Velocity += Camera.CameraForward * GamePad.GetState(Game_Core.CurrentLocal).Triggers.Left * -(ViewOffset + 11.4415007f);
                Velocity += Camera.CameraUp * GamePad.GetState(Game_Core.CurrentLocal).Triggers.Left * 6;
            }
            Game_Core.ClearedLevel = GamePad.GetState(Game_Core.CurrentLocal).IsButtonDown(Buttons.B);
            CurrentWeapon.Update();
            if (Switchcooldown > 0) Switchcooldown--;
            
            //if (GamePad.GetState(Game_Core.CurrentLocal).IsButtonDown(Buttons.Y))
            //{
            //    Velocity += Camera.CameraForward * 10;
            //}
            if (JumpCooldown > 0)
            {
                Velocity += (JumpPower * JumpCooldown) * Camera.CameraUp.Y;
                JumpCooldown -= 0.2f;
            }
            //CollisionChecks
            if (!OnGround)
            {
                //Ray TestGround = new Ray(Collision.Center,Vector3.Down * 10);
                //Vector3 TestGroundVec = Level.GetIntersectingPoly(TestGround, (Vector3.Down * 10).LengthSquared());
                //if (TestGroundVec == Vector3.Zero)
                //{
                Gravity = Camera.CameraUp * -3;
                Velocity += Gravity;
                //ScreenTextManager.RenderText("Gravity", new Vector2(10, 70), Color.White);
                //}
                //else
                // {
                //   OnGround = true;
                //}
                FallTimer++;
            }
            else
            {
                if (FallTimer > 10)
                {
                    RumbleCore.Rumble(5, 300);
                }
                FallTimer = 0;
            }
            Level.SphereColliding(ref Collision, ref Velocity, out OnGround);
            Velocity = Vector3.Zero;
            if (cameraPosition.Y - Collision.Center.Y + 10 > 5 || cameraPosition.Y - Collision.Center.Y + 10 < -5)
            {
                cameraPosition = Collision.Center + new Vector3(40 * (float)Math.Sin(YRotation), Camera.CameraUp.Y * 10, 40 * (float)Math.Cos(YRotation));
            }
            else
            {
                cameraPosition.X = Collision.Center.X + 40 * (float)(Math.Sin(YRotation));
                cameraPosition.Z = Collision.Center.Z + 40 * (float)(Math.Cos(YRotation));
                cameraPosition.Y = Collision.Center.Y + Camera.CameraUp.Y * 10;

            }
            Camera.CreateLookAt(Collision.Center, cameraPosition + new Vector3(0, ViewOffset, 0));

            //ScreenTextManager.RenderText(Health.ToString(), new Vector2(10, 100), Color.White);

//            AvatarFactory.Position = Collision.Center;
  //          AvatarFactory.Rotation = PlayerRotation;
            //Check for doors
            int DoorTarget = 0;
            String DoorCheck = Level.NearDoor(Collision.Center, out DoorTarget);
            if (DoorCheck != "")
            {
                SpriteManager.RenderSprite(0, new Vector2(600, 600));
                if (GamePad.GetState(Game_Core.CurrentLocal).Buttons.Y == ButtonState.Pressed && DoorCooldown == 0)
                {
                    Game_Core.ClearGame();
                    Level.LevelLoaded = false;
                    Collision.Center = Level.LoadLevel(DoorCheck, DoorTarget);
                    DoorCooldown = 100;
                    
                }
            }
            if (DoorCooldown > 0)
            {
                DoorCooldown--;
            }
            for (int i = 0; i < GrenadeCount; i++)
            {
                SpriteManager.RenderSprite(11,new Vector2(1110 - (i * 70),560));
            }
            SpriteManager.RenderSprite(13, new Vector2(444, 608));
            for (int i = 0; i < Health; i += 5)
            {
                SpriteManager.RenderSprite(12, new Vector2(480 + (i * 3), 640));
            }
            if (Collision.Center.Y < -130)
            {
                Health--;
            }
        }
        public void SetPosition(Vector3 Position)
        {
            Collision.Center = Position;
        }
    }
}
