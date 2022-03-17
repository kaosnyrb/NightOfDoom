using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace KNPE
{
    class AvatarFactory
    {
        public static Vector3 Position = new Vector3(0, 0, 0);
        public static Matrix Rotation = Matrix.Identity;
#if XBOX
        public static AvatarDescription Description = AvatarDescription.CreateRandom(AvatarBodyType.Male);
        public static AvatarAnimation Animation = new AvatarAnimation(AvatarAnimationPreset.MaleIdleLookAround);

        public static AvatarExpression Expression = new AvatarExpression();
        //public static Avatar Player = new Avatar(AvatarDescription.CreateRandom(AvatarBodyType.Male));

        public static void LoadContent()
        {
            //Model Ani1 = Game1.ContentMan.Load<Model>("your_skinned_model");
            //Player.StartAnimation(Ani1, "", true);
        }

        public static void Init()
        {
//            Expression.LeftEye = AvatarEye.Neutral;
  //          Expression.RightEye = AvatarEye.Neutral;
    //        Expression.Mouth = AvatarMouth.Neutral;
      //      Expression.LeftEyebrow = AvatarEyebrow.Neutral;
        //    Expression.RightEyebrow = AvatarEyebrow.Neutral;
        }
#endif
        public static void Draw(GameTime Time)
        {
#if XBOX
  //          Animation.Update(Time.ElapsedGameTime, true);
    //        AvatarRenderer Render = new AvatarRenderer(Description, true);
      //      Render.View = Camera.GetViewMatrix();
        //    Render.World = Rotation * Matrix.CreateTranslation(Position);
          //  Render.Draw(Animation.BoneTransforms, Expression);
#endif
        }
    }

}
